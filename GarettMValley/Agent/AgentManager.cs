
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using GarettMValley.Agent.Adapters;
using GarettMValley.Agent.Core;
using GarettMValley.Agent.Mind;
using GarettMValley.Agent.Mind.Personality;
using GarettMValley.Agent.Stimuli;
using GarettMValley.Agent.Runtime;
using GarettMValley.Dialogue;
using StardewValley.Characters;

namespace GarettMValley.Agent;

/// <summary>
/// Manager for AI Agents
/// </summary>
public sealed partial class AgentManager
{
    /// <summary>
    /// Reference to our logging instance
    /// </summary>
    private readonly IMonitor _monitor;

    /// <summary>
    /// Reference to our stimulus bus
    /// </summary>
    private readonly StimulusBus _stimulusBus = new();

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

    /// <summary>
    /// Active squared radius around the player
    /// </summary>
    private const float ActiveRadiusTilesSq = ActiveRadiusTiles * ActiveRadiusTiles;

    /// <summary>
    /// Personality profiles for agents
    /// </summary>
    private PersonalityLibrary _personalities = null!;

    /// <summary>
    /// Personality Autogen Helper
    /// </summary>
    private PersonalityFileGen _personalityAutogen = null!;


    /// <summary>
    /// Construct a new AiManager
    /// </summary>
    /// <param name="monitor">Logging instance</param>
    public AgentManager(IMonitor monitor, ModConfig config)
    {
        _monitor = monitor;
        _config = config;
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
            _debugAgentText.Remove(id);
        }
    }


    /// <summary>
    /// Try getting a runtime for a given NPC
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="mindstate"></param>
    /// <param name="currentActionKey"></param>
    /// <param name="intentKey"></param>
    /// <returns></returns>
    public bool TryGetRuntimeForNpc(NPC npc, out MindState? mindstate, out string? currentActionKey, out string? intentKey)
    {
        mindstate = null;
        currentActionKey = null;
        intentKey = null;

        var id = $"npc:{npc.Name}";
        if (!_agents.TryGetValue(id, out var agentRuntime))
            return false;

        mindstate = agentRuntime.GetMindState();
        currentActionKey = agentRuntime.GetCurrentAction()?.IntentKey;
        intentKey = mindstate.IntentKey;
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

            runtime = new AgentRuntime(adapter, _personalities, _monitor);
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
}