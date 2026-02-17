using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace GarettMValley.AI;

/// <summary>
/// Agent AI Stimulus Types
/// </summary>
public enum StimulusType
{
    /// <summary>
    /// Player entity is present
    /// </summary>
    PlayerPresense,
    /// <summary>
    /// Threat entity is present
    /// </summary>
    Threat,
    /// <summary>
    /// Friendly entity is present
    /// </summary>
    Friendly,
    /// <summary>
    /// Noise entity is present
    /// </summary>
    Noise,
    /// <summary>
    /// Pet entity is present
    /// </summary>
    Pet,
    /// <summary>
    /// Monster entity is present
    /// </summary>
    Monster,
}

/// <summary>
/// Stimulus object
/// </summary>
/// <param name="type"></param>
/// <param name="sourceId"></param>
/// <param name="locationName"></param>
/// <param name="tile"></param>
/// <param name="intensity"></param>
/// <param name="tags"></param>
public sealed record Stimulus(StimulusType type, string sourceId, string locationName, Vector2 tile, float intensity, string[] tags);