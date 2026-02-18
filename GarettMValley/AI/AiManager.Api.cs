using System;
using System.Collections.Generic;
using System.Linq;
using GarettMValley.AI.Api;
using StardewModdingAPI;
using StardewValley;

namespace GarettMValley.AI;

public sealed partial class AiManager
{
    private volatile AgentWorldSnapshot _latestSnapshot = new(
        Utc: DateTime.UtcNow,
        WorldReady: false,
        Location: null,
        Season: null,
        DayOfMonth: 0,
        TimeOfDay: 0,
        UniqueMultiplayerID: 0,
        StimuliCount: 0,
        Agents: Array.Empty<AgentSnapshot>()
    );

    /// <summary>
    /// Get the latest cached snapshot from the debug HTTP API.
    /// This is safe to call from any thread.
    /// </summary>
    /// <returns></returns>
    public AgentWorldSnapshot GetLatestSnapshot() => _latestSnapshot;

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
            var bb = runtime.GetBlackboard();
            var emotion = bb.Emotion;
            var personality = bb.Personality;
            var moveTarget = bb.MoveTargetTile;

            string? name = null;
            try
            {
                // Character.Name is usually safe, but keep try/catch defensive.
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
                IntentKey: bb.IntentKey,
                IntentTicksLeft: bb.IntentTicksLeft,
                DistanceToPlayerTiles: bb.DistanceToPlayerTiles,
                PlayerIsNear: bb.PlayerIsNear,
                ThreatNearby: bb.ThreatNearby,
                ThreatTileX: bb.ThreatTile.X,
                ThreatTileY: bb.ThreatTile.Y,
                NearbyAgentsCount: bb.NearbyAgentsCount,
                NearestFriendlyId: bb.NearestFriendlyId,
                NearestFriendlyTileX: bb.NearestFriendlyTile.X,
                NearestFriendlyTileY: bb.NearestFriendlyTile.Y,
                DistToNearestFriendlyTiles: bb.DistToNearestFriendlyTiles,
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
            StimuliCount: worldReady && location is not null ? _stimulusBus.DebugCountForLocation(location.NameOrUniqueName) : 0,
            Agents: list
        );
    }
}