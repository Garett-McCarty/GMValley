using GarettMValley.Agent.Core;
using GarettMValley.Agent.Mind.Personality;
using GarettMValley.Agent.Stimuli;
using StardewModdingAPI;
using StardewValley;

namespace GarettMValley.Agent.Runtime;

/// <summary>
/// Agent Runtime
/// </summary>
internal sealed class AgentRuntime
{
    /// <summary>
    /// How often agents should update
    /// </summary>
    private const int TickInterval = 15; // 15 ticks ~= 4 times/sec (60 ticks/sec)
    
    /// <summary>
    /// The active agent adapter
    /// </summary>
    private IAgentAdapter _adapter;

    /// <summary>
    /// Agents mindstate
    /// </summary>
    private readonly MindState _mindState = new();

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
    private readonly IMonitor _monitor;

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

    /// <summary>
    /// Get the adapter, used for debugging.
    /// </summary>
    /// <returns></returns>
    public IAgentAdapter GetAdapterForDebug() => _adapter;

    /// <summary>
    /// Agent constructor
    /// </summary>
    /// <param name="adapter">Agent controller</param>
    /// <param name="personalities">Agent personalities</param>
    /// <param name="monitor">Log instance</param>
    public AgentRuntime(IAgentAdapter adapter, PersonalityLibrary personalities, IMonitor monitor)
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
        _monitor = monitor;
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
        _mindState.BeginTick(location, _adapter, player, bus);

        // 2. Personality assignment
        var characterName = GetCharacterName(_adapter);
        _mindState.Personality = _personalities.GetForCharacterName(characterName);

        // 3. Emotional decay
        const float gameTicksPerSecond = 60.0f;
        float delta = TickInterval / gameTicksPerSecond;
        _mindState.Emotion.DecayToward(baseline: _mindState.Personality.Baseline, delta, halflife: _mindState.Personality.EmotionalHalfLife);

        // 4. Sensors apply emotional deltas, etc.
        foreach (var sensor in _sensors)
            sensor.Sense(_mindState);
        if (_current is not null && _mindState.HasIntent && _current.IntentKey == _mindState.IntentKey && !_current.IsFinished)
        {
            if (_current.CanContinue(_mindState))
            {
                _current.Tick(_mindState);
                return;
            }

            _current.Abort(_mindState);
            _current = null;
            _mindState.ClearIntent();
        }
        var next = _brain.ChooseAction(_mindState);
        if (next is not null)
        {
            var key = next.IntentKey;
            if (_lastActionKey != key)
            {
                _lastActionKey = key;
                _monitor.Log($"AI[{_adapter.Id.Value}] {(_adapter.Kind)} -> {key} (intent={_mindState.IntentKey}:{_mindState.IntentTicksLeft})", LogLevel.Info);
            }
        }
        _current = next;
        _current?.Start(_mindState);
        _current?.Tick(_mindState);
    }

    /// <summary>
    /// Get debug line info
    /// </summary>
    /// <returns></returns>
    public string GetDebugLine()
    {
        var emotion = _mindState.Emotion;
        return $"{_adapter.Kind} {GetCharacterName(_adapter)}\n" +
            $"act={_current?.IntentKey ?? "none"} intent={_mindState.IntentKey}:{_mindState.IntentTicksLeft}\n" +
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

    /// <summary>
    /// Determine if a player is nearby
    /// </summary>
    /// <returns></returns>
    public bool DebugPlayerIsNear() => _mindState.PlayerIsNear;

    public string BuildDialogueLine(NPC npc, Farmer who, GameLocation location)
    {
        // placeholder: later you’ll call your Ollama prompt builder heres
        return $"({npc.Name}) mood={_mindState.Emotion.Valence:0.00}/{_mindState.Emotion.Arousal:0.00} intent={_mindState.IntentKey}";
    }

    public MindState GetMindState() => _mindState;
    public IAction? GetCurrentAction() => _current;
}