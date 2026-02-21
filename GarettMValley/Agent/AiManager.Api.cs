using System;
using System.Collections.Generic;
using System.Linq;
using GarettMValley.Agent.Api;
using StardewModdingAPI;
using StardewValley;

namespace GarettMValley.Agent;

public sealed partial class AiManager
{
    /// <summary>
    /// Latest agent world snapshot
    /// </summary>
    private volatile AgentWorldSnapshot _latestSnapshot = new(
        Utc: DateTime.UtcNow,
        WorldReady: false,
        Location: null,
        Season: null,
        DayOfMonth: 0,
        TimeOfDay: 0,
        UniqueMultiplayerID: 0,
        StimuliCount: 0,
        Agents: Array.Empty<AgentSnapshot>(),
        StimuliByLocation: null
    );

    /// <summary>
    /// Tic accumulator for tracking how often we update debug snapshot information
    /// </summary>
    private int _nextApiSnapshotTick = 0;

    /// <summary>
    /// Get the latest cached snapshot from the debug HTTP API.
    /// This is safe to call from any thread.
    /// </summary>
    /// <returns></returns>
    public AgentWorldSnapshot GetLatestSnapshot() => _latestSnapshot;

    /// <summary>
    /// Attempt to get an AgentSnapshot for a given unique id
    /// </summary>
    /// <param name="id">Unique identifier of the agent</param>
    /// <returns></returns>
    public AgentSnapshot? TryGetAgentSnapshot(string id)
    {
        var snap = _latestSnapshot;
        return snap.Agents.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Rebuild cached snapshot. Must be called from the game thread.
    /// </summary>
    private void UpdateApiSnapshot(GameLocation location)
    {
        // Build list eagerly so the resulting snapshot is immutable.
        var list = new List<AgentSnapshot>(_agents.Count);
        foreach (var (id, runtime) in _agents)
        {
            var adapter = runtime.GetAdapterForDebug();
            var blackboard = runtime.GetBlackboard();
            var emotion = blackboard.Emotion;
            var personality = blackboard.Personality;
            var moveTarget = blackboard.MoveTargetTile;

            string? name = null;
            try
            {
                name = adapter.Character?.Name;
            }
            catch { /* ignored */ }

            list.Add(new AgentSnapshot(
                Id: id,
                Kind: adapter.Kind,
                Name: name,
                Location: adapter.Location?.NameOrUniqueName,
                TileX: adapter.Tile.X,
                TileY: adapter.Tile.Y,
                CurrentAction: runtime.GetCurrentAction()?.IntentKey,
                IntentKey: blackboard.IntentKey,
                IntentTicksLeft: blackboard.IntentTicksLeft,
                DistanceToPlayerTiles: blackboard.DistanceToPlayerTiles,
                PlayerIsNear: blackboard.PlayerIsNear,
                ThreatNearby: blackboard.ThreatNearby,
                ThreatTileX: blackboard.ThreatTile.X,
                ThreatTileY: blackboard.ThreatTile.Y,
                NearbyAgentsCount: blackboard.NearbyAgentsCount,
                NearestFriendlyId: blackboard.NearestFriendlyId,
                NearestFriendlyTileX: blackboard.NearestFriendlyTile.X,
                NearestFriendlyTileY: blackboard.NearestFriendlyTile.Y,
                DistToNearestFriendlyTiles: blackboard.DistanceToNearestFriendly,
                MoveTargetTileX: moveTarget?.X,
                MoveTargetTileY: moveTarget?.Y,
                Emotion: new EmotionSnapshot(
                    Valence: emotion.Valence,
                    Arousal: emotion.Arousal,
                    Dominance: emotion.Dominance,
                    Stress: emotion.Stress,
                    SocialNeed: emotion.SocialNeed,
                    Curiosity: emotion.Curiosity,
                    Fatigue: emotion.Fatigue
                ),
                Personality: new PersonalitySnapshot(
                    ProfileKey: personality.Key,
                    BaselineValence: personality.Baseline.Valence,
                    BaselineArousal: personality.Baseline.Arousal,
                    BaselineDominance: personality.Baseline.Dominance,
                    BaselineStress: personality.Baseline.Stress,
                    EmotionalHalfLife: personality.EmotionalHalfLife
                )
            ));
        }

        bool worldReady = Context.IsWorldReady;

        _latestSnapshot = new AgentWorldSnapshot(
            Utc: DateTime.UtcNow,
            WorldReady: worldReady,
            Location: location?.NameOrUniqueName,
            Season: worldReady ? Game1.currentSeason : null,
            DayOfMonth: worldReady ? Game1.dayOfMonth : 0,
            TimeOfDay: worldReady ? Game1.timeOfDay : 0,
            UniqueMultiplayerID: worldReady ? (long)Game1.player.UniqueMultiplayerID : 0,
            StimuliCount: worldReady && location is not null ? _stimulusBus.DebugCountByLocation(location.NameOrUniqueName) : 0,
            Agents: list,
            StimuliByLocation: worldReady ? _stimulusBus.DebugCountAllByLocation() : null
        );
    }
}