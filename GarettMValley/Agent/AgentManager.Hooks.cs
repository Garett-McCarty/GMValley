
using GarettMValley.Agent.Mind;
using GarettMValley.Agent.Mind.Personality;
using GarettMValley.Agent.Runtime;
using GarettMValley.Agent.Stimuli;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace GarettMValley.Agent;

/// <summary>
/// Game Hook related portion of the AgentManager
/// </summary>
public sealed partial class AgentManager
{
    /// <summary>
    /// Hook into our game loop tick and player warped events
    /// </summary>
    /// <param name="helper"></param>
    public void Hook(IModHelper helper)
    {
        _personalityAutogen = new PersonalityFileGen(helper, _monitor);
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
            _monitor.Log("Generating AI personalities off game data...", LogLevel.Info);
            _personalityAutogen.GenerateFile(force: true);
            _monitor.Log("Generated AI personalities off game data!", LogLevel.Info);
        }
        if (e.Button == SButton.F9)
            _debug = !_debug;
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
        _stimulusBus.KeepOnlyLocation(e.NewLocation.NameOrUniqueName);
        _agents.Clear();
        UpdateApiSnapshot(e.NewLocation);
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

        bool debugPulse = _debug && e.IsMultipleOf(DebugEveryTicks);

        if (e.Ticks >= _nextApiSnapshotTick)
        {
            _nextApiSnapshotTick = (int)e.Ticks + 60;
        }

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
                _agents[id] = new AgentRuntime(adapter, _personalities!, _monitor);
            else
                runtime.SetAdapter(adapter);
        }

        // 3) Publish player presence stimulus (once)
        _stimulusBus.Publish(new PlayerStimulus(Tick: 0, LocationName: location.NameOrUniqueName, Tile: player.Tile, Intensity: 1.0f));

        // 4) Tick only "active" agents near the player
        foreach (var adapter in _gatherBuffer)
        {
            var id = adapter.Id.Value;
            if (Vector2.DistanceSquared(adapter.Tile, player.Tile) > ActiveRadiusTilesSq)
                continue;

            if (_agents.TryGetValue(id, out var runtime))
            {
                runtime.Tick(location, player, _stimulusBus);
                _debugAgentText[id] = runtime.GetDebugLine();
            }
            else
            {
                _debugAgentText[id] = "rt-missing";
            }
            
        }

        // 5) Remove agents that no longer exist in this location
        CleanupMissing(_gatherBuffer);

        if (e.Ticks >= _nextApiSnapshotTick)
        {
            UpdateApiSnapshot(location);
        }
    }

    /// <summary>
    /// Hook into OnRenderedHud to display some debug info if if debug mode is enabled.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!_debug || !Context.IsWorldReady)
            return;
        var location = Game1.player?.currentLocation;
        if (location is null)
            return;
        string label_text = $"AI Debug\nLocation: {location.NameOrUniqueName}\nAgents: {_agents.Count}\nStimuli: {_stimulusBus.DebugCount()}";
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
        if (!_debug || !Context.IsWorldReady)
            return;
        var location = Game1.currentLocation;
        if (location is null)
            return;
        foreach (var (id, runtime) in _agents)
        {
            var adapter = runtime.GetAdapterForDebug();
            if (Vector2.DistanceSquared(adapter.Tile, Game1.player.Tile) > ActiveRadiusTilesSq)
                continue;
            if (!_debugAgentText.TryGetValue(id, out var text))
                continue;
            DrawAgentLabel(e.SpriteBatch, adapter, text);
        }
    }
}