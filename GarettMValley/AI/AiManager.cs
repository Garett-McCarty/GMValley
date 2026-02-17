using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using GarettMValley.AI.Adapters;
using GarettMValley.AI.Mind;
using StardewValley.Characters;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace GarettMValley.AI;

/// <summary>
/// Manager for AI Agents
/// </summary>
public sealed partial class AiManager
{
    /// <summary>
    /// Reference to our logging instance
    /// </summary>
    private readonly IMonitor _log;

    /// <summary>
    /// Reference to our configuration
    /// </summary>
    private readonly ModConfig _config;

    /// <summary>
    /// Reference to our stimulus bus
    /// </summary>
    private readonly StimulusBus _stimulusBus = new();

    /// <summary>
    /// Reference to our agents
    /// </summary>
    private readonly Dictionary<string, AgentRuntime> _agents = new();

    /// <summary>
    /// Reusable list for gathering agents
    /// </summary>
    private readonly List<IAgentAdapter> _gatherBuffer = new();

    /// <summary>
    /// How often agents should update
    /// </summary>
    private const int TickInterval = 15; // 15 ticks ~= 4 times/sec (60 ticks/sec)

    /// <summary>
    /// Active agents around the player
    /// </summary>
    private const float ActiveRadiusTiles = 20f;
    private const float ActiveRadiusTilesSq = ActiveRadiusTiles * ActiveRadiusTiles;

    /// <summary>
    /// Debug AI information
    /// </summary>
    private bool Debug = true;

    /// <summary>
    /// Print about once/sec
    /// </summary>
    private uint DebugEveryTicks = 60;

    /// <summary>
    /// Debug text for debug mode
    /// </summary>
    private readonly Dictionary<string, string> DebugAgentText = new();

    /// <summary>
    /// Personality profiles for agents
    /// </summary>
    private PersonalityLibrary _personalities = null!;

    private PersonalityAutogen _personalityAutogen = null!;


    /// <summary>
    /// Construct a new AiManager
    /// </summary>
    /// <param name="log">Logging instance</param>
    public AiManager(IMonitor log, ModConfig config)
    {
        _log = log;
        _config = config;
    }

    /// <summary>
    /// Hook into our game loop tick and player warped events
    /// </summary>
    /// <param name="helper"></param>
    public void Hook(IModHelper helper)
    {
        _personalityAutogen = new PersonalityAutogen(helper, _log);
        _personalities = new PersonalityLibrary(helper);

        helper.Events.Input.ButtonPressed += OnButtonPressed;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.Player.Warped += this.OnWarped;
        helper.Events.Display.RenderedHud += this.OnRenderedHud;
        helper.Events.Display.RenderedWorld += this.OnRenderedWorld;
    }

    /// <summary>
    /// Hook into OnButtonPressed to check for input related to toggling debug mode.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;
        if (e.Button == SButton.F8)
        {
            _log.Log("Generating AI personalities off game data...", LogLevel.Info);
            _personalityAutogen.GenerateFile(force: true);
            _log.Log("Generated AI personalities off game data!", LogLevel.Info);
        }
        if (e.Button == SButton.F9)
            Debug = !Debug;
        if (e.Button == SButton.F10)
            _agents.Clear();
    }

    /// <summary>
    /// Handler for when a player is warped
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnWarped(object? sender, WarpedEventArgs e)
    {
        _stimulusBus.ClearLocation(e.NewLocation?.NameOrUniqueName);
        _agents.Clear();
    }

    /// <summary>
    /// Handler for when a process tick is updated in the game loop
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;
        if (!e.IsMultipleOf(TickInterval))
            return;

        bool debugPulse = Debug && e.IsMultipleOf(DebugEveryTicks);

        var player = Game1.player;
        var location = player.currentLocation;
        if (location is null)
            return;

        // 1) Gather everyone in the location (one pass, reuse buffer)
        _gatherBuffer.Clear();
        GatherAgents(location, _gatherBuffer);

        // 2) Ensure one AgentRuntime per character
        foreach (var adapter in _gatherBuffer)
        {
            var id = adapter.Id.Value;
            if (!_agents.TryGetValue(id, out var runtime))
                _agents[id] = new AgentRuntime(adapter, _personalities!, _log);
            else
                runtime.SetAdapter(adapter);
        }

        // 3) Publish player presence stimulus (once)
        _stimulusBus.Publish(new Stimulus(
            type: StimulusType.PlayerPresense,
            sourceId: "player",
            locationName: location.NameOrUniqueName,
            tile: player.Tile,
            intensity: 1.0f,
            tags: new[] { "player" }
        ));

        // 4) Tick only "active" agents near the player
        foreach (var adapter in _gatherBuffer)
        {
            var id = adapter.Id.Value;
            if (Vector2.DistanceSquared(adapter.Tile, player.Tile) > ActiveRadiusTilesSq)
                continue;

            if (_agents.TryGetValue(id, out var runtime))
            {
                runtime.Tick(location, player, _stimulusBus);
                DebugAgentText[id] = runtime.GetDebugLine();
            }
            else
            {
                DebugAgentText[id] = "rt-missing";
            }
            
        }

        // 5) Remove agents that no longer exist in this location
        CleanupMissing(_gatherBuffer);
    }

    /// <summary>
    /// Hook into OnRenderedHud to display some debug info if if debug mode is enabled.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!Debug || !Context.IsWorldReady)
            return;
        var location = Game1.player?.currentLocation;
        if (location is null)
            return;
        string label_text = $"AI Debug\nLocation: {location.NameOrUniqueName}\nAgents: {_agents.Count}\nStimuli: {_stimulusBus.DebugCountForLocation(location.NameOrUniqueName)}";
        var position = new Vector2(16.0f, 16.0f);
        e.SpriteBatch.DrawString(Game1.smallFont, text: label_text, position, Color.White);
    }

    /// <summary>
    /// Hook into OnRenderedWorld to display some debug info if if debug mode is enabled.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (!Debug || !Context.IsWorldReady)
            return;
        var location = Game1.currentLocation;
        if (location is null)
            return;
        foreach (var (id, runtime) in _agents)
        {
            var adapter = runtime.GetAdapterForDebug();
            if (Vector2.DistanceSquared(adapter.Tile, Game1.player.Tile) > ActiveRadiusTilesSq)
                continue;
            if (!DebugAgentText.TryGetValue(id, out var text))
                continue;
            DrawAgentLabel(e.SpriteBatch, adapter, text);
        }
    }

    /// <summary>
    /// Gather a list of Agents in a given location (reuses provided list)
    /// </summary>
    /// <param name="location"></param>
    /// <param name="buffer">List to populate with agents</param>
    private void GatherAgents(GameLocation location, List<IAgentAdapter> buffer)
    {
        foreach (var character in location.characters)
        {
            if (character is null) continue;
            if (character is Monster monster) buffer.Add(new MonsterAdapter(monster));
            else if (character is Pet pet) buffer.Add(new PetAdapter(pet));
            else if (character is NPC npc) buffer.Add(new VillagerAdapter(npc));
        }
    }

    /// <summary>
    /// Remove queued agents from our active agent list
    /// </summary>
    /// <param name="activeAdapters"></param>
    private void CleanupMissing(List<IAgentAdapter> activeAdapters)
    {
        var active = new HashSet<string>(activeAdapters.Count);
        foreach (var adapter in activeAdapters)
            active.Add(adapter.Id.Value);
        
        var toRemove = new List<string>();
        foreach (var id in _agents.Keys)
        {
            if (!active.Contains(id))
                toRemove.Add(id);
        }

        foreach (var id in toRemove)
        {
            _agents.Remove(id);
            DebugAgentText.Remove(id);
        }
    }

    /// <summary>
    /// Draw a label over a given agent
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="adapter"></param>
    /// <param name="text"></param>
    private void DrawAgentLabel(SpriteBatch spriteBatch, IAgentAdapter adapter, string text)
    {
        if (adapter.Raw is not StardewValley.Character character)
            return;

        // don't draw our debug info if the character is offscreen
        Rectangle viewportRect = new Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height);
        if (!viewportRect.Intersects(character.GetBoundingBox()))
            return;

        // World position (center top of character bounding box)
        var boundingBox = character.GetBoundingBox();

        Vector2 worldPosition = new Vector2(
            boundingBox.Center.X,
            boundingBox.Top - 32.0f // slightly above head
        );

        var lines = text.Count(c => c == '\n') + 1;
        worldPosition.Y -= (lines - 1) * 10.0f;

        // Convert world → screen
        Vector2 screenPosition = Game1.GlobalToLocal(Game1.viewport, worldPosition);

        // Center text
        var size = Game1.smallFont.MeasureString(text);
        screenPosition.X -= size.X / 2f;

        // Draw shadow for readability
        spriteBatch.DrawString(Game1.smallFont, text, screenPosition + new Vector2(1, 1), Color.Black * 0.75f);
        spriteBatch.DrawString(Game1.smallFont, text, screenPosition, Color.Yellow);
    }


    /// <summary>
    /// Try getting a runtime for a given NPC
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="blackboard"></param>
    /// <param name="currentActionKey"></param>
    /// <param name="intentKey"></param>
    /// <returns></returns>
    public bool TryGetRuntimeForNpc(NPC npc, out Blackboard? blackboard, out string? currentActionKey, out string? intentKey)
    {
        blackboard = null;
        currentActionKey = null;
        intentKey = null;

        var id = $"npc:{npc.Name}";
        if (!_agents.TryGetValue(id, out var agentRuntime))
            return false;

        blackboard = agentRuntime.GetBlackboard();
        currentActionKey = agentRuntime.GetCurrentAction()?.IntentKey;
        intentKey = blackboard.IntentKey;
        return true;
    }

    /// <summary>
    /// Decide whether to take over dialogue for this NPC interaction.
    /// </summary>
    public bool TryDecideTakeover(NPC npc, Farmer who, GameLocation location, out DialogueTakeover takeover)
    {
        takeover = default;

        // Require an agent runtime (create if missing)
        var adapter = new Adapters.VillagerAdapter(npc);
        var id = adapter.Id.Value;

        if (!_agents.TryGetValue(id, out var runtime))
        {
            if (_personalities is null)
                return false;

            runtime = new AgentRuntime(adapter, _personalities, _log);
            _agents[id] = runtime;
        }
        else
        {
            runtime.SetAdapter(adapter);
        }

        // Tick once so the line reflects latest emotion/sensors/intent.
        // (This is “cheap” since you already tick on UpdateTicked; but it makes talk feel responsive.)
        runtime.Tick(location, who, _stimulusBus);

        // --- Decision policy (start simple) ---
        // Example: only take over if player is near and agent is in a "social" mood,
        // or if you want to always take over when pressed.
        // For now: take over when NOT holding an item, and player is near (<= 4 tiles).
        bool playerNear = runtime.DebugPlayerIsNear();

        if (!playerNear)
        {
            takeover = new DialogueTakeover(false, "");
            return true;
        }

        // Build a line. For now: mix vanilla-ish + debug.
        // Later: call your Ollama server and store pending result.
        string line = runtime.BuildDialogueLine(npc, who, location);

        takeover = new DialogueTakeover(true, line);
        return true;
    }

    public ModConfig GetConfig() => _config;

    private sealed class AgentRuntime
    {
        /// <summary>
        /// The active agent adapter
        /// </summary>
        private IAgentAdapter _adapter;
        /// <summary>
        /// Agents blackboard
        /// </summary>
        private readonly Blackboard _blackboard = new();
        /// <summary>
        /// List of sensor inputs
        /// </summary>
        private readonly List<ISensor> _sensors;
        /// <summary>
        /// BrainUtility for the agent
        /// </summary>
        private readonly BrainUtility _brain;
        /// <summary>
        /// Personality references for agents
        /// </summary>
        private readonly PersonalityLibrary _personalities;
        /// <summary>
        /// Logging instance
        /// </summary>
        private readonly IMonitor _log;
        /// <summary>
        /// Current action being performed by the agent
        /// </summary>
        private IAction? _current;
        /// <summary>
        /// Last action executed by the agent
        /// </summary>
        private string? _lastActionKey;

        /// <summary>
        /// Set the adapter controller for the agent
        /// </summary>
        /// <param name="adapter">Adapter responsible for the agent. MonsterAdapter, PetAdapter, VillagerAdapter, etc</param>
        public void SetAdapter(IAgentAdapter adapter) => _adapter = adapter;
        public IAgentAdapter GetAdapterForDebug() => _adapter;
        public string BuildDialogueLine(bool debug = false)
        {
            // TODO: Replace with Ollama later.
            var emotion = _blackboard.Emotion;

            string mood =
                emotion.Stress > 0.65f ? "…I’m a little on edge today." :
                emotion.Valence > 0.35f ? "Nice to see you!" :
                emotion.Valence < -0.35f ? "Oh. Hey." :
                "Hi.";

            if (!debug)
                return $"{mood}";
            // Optionally add debug as a second page using #$e#
            // (Stardew uses #$e# as a dialogue page break in many assets.)
            string message = GetDebugLine();
            return $"{mood}#$e#{message}";
        }

        /// <summary>
        /// Agent constructor
        /// </summary>
        /// <param name="adapter">Agent controller</param>
        /// <param name="personalities">Agent personalities</param>
        /// <param name="log">Log instance</param>
        public AgentRuntime(IAgentAdapter adapter, PersonalityLibrary personalities, IMonitor log)
        {
            _adapter = adapter;
            _sensors = new()
            {
                new Sensors.PlayerProximitySensor(),
                new Sensors.NearbyThreatSensor(),
                new Sensors.NearbyAgentSensor(),
            };
            _brain = new BrainUtility();
            _personalities = personalities;
            _log = log;
        }

        /// <summary>
        /// Tick the agent
        /// </summary>
        /// <param name="location"></param>
        /// <param name="player"></param>
        /// <param name="bus"></param>
        public void Tick(GameLocation location, Farmer player, StimulusBus bus)
        {
            // 1. Tick the Agents "awareness"
            _blackboard.BeginTick(location, _adapter, player, bus);

            // 2. Personality assignment
            var characterName = GetCharacterName(_adapter);
            _blackboard.Personality = _personalities.GetForCharacterName(characterName);

            // 3. Emotional decay
            const float gameTicksPerSecond = 60.0f;
            float delta = TickInterval / gameTicksPerSecond;
            _blackboard.Emotion.DecayToward(baseline: _blackboard.Personality.Baseline, delta, halflife: _blackboard.Personality.EmotionalHalfLife);

            // 4. Sensors apply emotional deltas, etc.
            foreach (var sensor in _sensors)
                sensor.Sense(_blackboard);
            if (_current is not null && _blackboard.HasIntent && _current.IntentKey == _blackboard.IntentKey && !_current.IsFinished)
            {
                if (_current.CanContinue(_blackboard))
                {
                    _current.Tick(_blackboard);
                    return;
                }

                _current.Abort(_blackboard);
                _current = null;
                _blackboard.ClearIntent();
            }
            var next = _brain.ChooseAction(_blackboard);
            if (next is not null)
            {
                var key = next.IntentKey;
                if (_lastActionKey != key)
                {
                    _lastActionKey = key;
                    _log.Log($"AI[{_adapter.Id.Value}] {(_adapter.Kind)} -> {key} (intent={_blackboard.IntentKey}:{_blackboard.IntentTicksLeft})", LogLevel.Info);
                }
            }
            _current = next;
            _current?.Start(_blackboard);
            _current?.Tick(_blackboard);
        }

        /// <summary>
        /// Get debug line info
        /// </summary>
        /// <returns></returns>
        public string GetDebugLine()
        {
            var emotion = _blackboard.Emotion;
            return $"{_adapter.Kind} {GetCharacterName(_adapter)}\n" +
                $"act={_current?.IntentKey ?? "none"} intent={_blackboard.IntentKey}:{_blackboard.IntentTicksLeft}\n" +
                $"V={emotion.Valence:0.00} A={emotion.Arousal:0.00} D={emotion.Dominance:0.00} S={emotion.Stress:0.00}\n" +
                $"Soc={emotion.SocialNeed:0.00} Cur={emotion.Curiosity:0.00} Fat={emotion.Fatigue:0.00}";
        }

        /// <summary>
        /// Get the character name from a given adapter
        /// </summary>
        /// <param name="adapter"></param>
        /// <returns></returns>
        private static string GetCharacterName(IAgentAdapter adapter)
        {
            if (adapter.Raw is StardewValley.Character character)
                return character.Name ?? adapter.Id.Value;
            return adapter.Id.Value;
        }

        public bool DebugPlayerIsNear() => _blackboard.PlayerIsNear;

        public string BuildDialogueLine(NPC npc, Farmer who, GameLocation location)
        {
            // placeholder: later you’ll call your Ollama prompt builder heres
            return $"({npc.Name}) mood={_blackboard.Emotion.Valence:0.00}/{_blackboard.Emotion.Arousal:0.00} intent={_blackboard.IntentKey}";
        }

        public Blackboard GetBlackboard() => _blackboard;
        public IAction? GetCurrentAction() => _current;
    }
}