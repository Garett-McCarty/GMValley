using System;
using System.Collections.Generic;

namespace GarettMValley.Agent.Api;

/// <summary>
/// JSON-friendly snapshot of the world + all known agents.
/// </summary>
/// <param name="Utc"></param>
/// <param name="WorldReady"></param>
/// <param name="Location"></param>
/// <param name="Season"></param>
/// <param name="DayOfMonth"></param>
/// <param name="TimeOfDay"></param>
/// <param name="UniqueMultiplayerID"></param>
/// <param name="StimuliCount"></param>
/// <param name="Agents"></param>
public sealed record AgentWorldSnapshot(
    DateTime Utc,
    bool WorldReady,
    string? Location,
    string? Season,
    int DayOfMonth,
    int TimeOfDay,
    long UniqueMultiplayerID,
    int StimuliCount,
    IReadOnlyList<AgentSnapshot> Agents,
    IReadOnlyDictionary<string, int>? StimuliByLocation = null
);

/// <summary>
/// JSON-friendly snapshot of a single agent
/// </summary>
/// <param name="Id"></param>
/// <param name="Kind"></param>
/// <param name="Name"></param>
/// <param name="Location"></param>
/// <param name="TileX"></param>
/// <param name="TileY"></param>
/// <param name="CurrentAction"></param>
/// <param name="IntentKey"></param>
/// <param name="IntentTicksLeft"></param>
/// <param name="DistanceToPlayerTiles"></param>
/// <param name="PlayerIsNear"></param>
/// <param name="ThreatNearby"></param>
/// <param name="ThreatTileX"></param>
/// <param name="ThreatTileY"></param>
/// <param name="NearbyAgentsCount"></param>
/// <param name="NearestFriendlyId"></param>
/// <param name="NearestFriendlyTileX"></param>
/// <param name="NearestFriendlyTileY"></param>
/// <param name="DistToNearestFriendlyTiles"></param>
/// <param name="MoveTargetTileX"></param>
/// <param name="MoveTargetTileY"></param>
/// <param name="Emotion"></param>
/// <param name="Personality"></param>
public sealed record AgentSnapshot(
    string Id,
    string Kind,
    string? Name,
    string? Location,
    float TileX,
    float TileY,
    string? CurrentAction,
    string? IntentKey,
    int IntentTicksLeft,
    float DistanceToPlayerTiles,
    bool PlayerIsNear,
    bool ThreatNearby,
    float ThreatTileX,
    float ThreatTileY,
    int NearbyAgentsCount,
    string? NearestFriendlyId,
    float NearestFriendlyTileX,
    float NearestFriendlyTileY,
    float DistToNearestFriendlyTiles,
    float? MoveTargetTileX,
    float? MoveTargetTileY,
    EmotionSnapshot Emotion,
    PersonalitySnapshot Personality
);

/// <summary>
/// JSON-friendly snapshot of an emotional state
/// </summary>
/// <param name="Valence"></param>
/// <param name="Arousal"></param>
/// <param name="Dominance"></param>
/// <param name="Stress"></param>
/// <param name="SocialNeed"></param>
/// <param name="Curiosity"></param>
/// <param name="Fatigue"></param>
public sealed record EmotionSnapshot(
    float Valence,
    float Arousal,
    float Dominance,
    float Stress,
    float SocialNeed,
    float Curiosity,
    float Fatigue
);

/// <summary>
/// JSON-friendly snapshot of a personality state
/// </summary>
/// <param name="ProfileKey"></param>
/// <param name="BaselineValence"></param>
/// <param name="BaselineArousal"></param>
/// <param name="BaselineDominance"></param>
/// <param name="BaselineStress"></param>
/// <param name="EmotionalHalfLife"></param>
public sealed record PersonalitySnapshot(
    string ProfileKey,
    float BaselineValence,
    float BaselineArousal,
    float BaselineDominance,
    float BaselineStress,
    float EmotionalHalfLife
);