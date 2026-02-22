
using GarettMValley.Agent.Adapters;
using GarettMValley.Agent.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace GarettMValley.Agent;

/// <summary>
/// Agent related portion of the AgentManager
/// </summary>
public sealed partial class AgentManager
{
    /// <summary>
    /// Reference to our agents
    /// </summary>
    private readonly Dictionary<string, AgentRuntime> _agents = new();

    /// <summary>
    /// Clear active agents being tracked by the agent manager
    /// </summary>
    public void ClearAgents()
    {
        _agents.Clear();
        _debugAgentText.Clear();
    }

    /// <summary>
    /// Regenerate personality file from game data.
    /// </summary>
    /// <param name="force"></param>
    public void RegeneratePersonalities(bool force = true)
    {
        _monitor.Log("Generating AI personalities off game data...", LogLevel.Info);
        _personalityAutogen.GenerateFile(force: force);
        _monitor.Log("Generated AI personalities off game data!", LogLevel.Info);
    }

    public bool TrySetNearestNpcEmotion(float valence, float arousal, float dominance, float stress, float radiusTiles, out string? npcName)
    {
        npcName = null;
        if (!Context.IsWorldReady)
            return false;
        var player = Game1.player;
        var location = player?.currentLocation;
        if (location is null)
            return false;
        NPC? nearestNpc = null;
        float bestSq = radiusTiles * radiusTiles;
        foreach (var character in location.characters)
        {
            if (character is NPC npc)
            {
                float distance = Vector2.DistanceSquared(npc.Tile, player!.Tile);
                if (distance <= bestSq)
                {
                    bestSq = distance;
                    nearestNpc = npc;
                }
            }
        }

        if (nearestNpc is null)
            return false;
        var adapter = new VillagerAdapter(nearestNpc);
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

        var mindState = runtime.GetMindState();
        mindState.Emotion.Valence = valence;
        mindState.Emotion.Arousal = arousal;
        mindState.Emotion.Dominance = dominance;
        mindState.Emotion.Stress = stress;
        mindState.Emotion.Normalize();
        npcName = nearestNpc.Name;
        return true;
    }
}