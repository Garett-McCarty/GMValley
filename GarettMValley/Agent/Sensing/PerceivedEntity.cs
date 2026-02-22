
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Perception;

/// <summary>
/// Perception Memory
/// </summary>
public sealed class PerceivedEntity
{
    /// <summary>
    /// Unique key (NPC.Name, "player", etc)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Optional label for debugging
    /// </summary>
    public string Kind { get; set; } = "unknown";

    /// <summary>
    /// Raw current tick visual score [0..1]
    /// </summary>
    public float See { get; set; }

    /// <summary>
    /// Smoothed memory / belief [0..1]
    /// </summary>
    public float Awareness { get; set; }

    /// <summary>
    /// Location of the memory
    /// </summary>
    public Vector2 LastSeenTile { get; set; }

    /// <summary>
    /// Time of the memory
    /// </summary>
    public int LastSeenTick { get; set; }

    /// <summary>
    /// Distance of how far the entity was
    /// </summary>
    public float DistanceTiles { get; set; }

    /// <summary>
    /// True if this entry was visually updated this tick
    /// </summary>
    public bool SeenThisTick { get; set; }
}