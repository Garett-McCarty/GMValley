
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Core;

/// <summary>
/// How the agent represents its body's state.
/// </summary>
internal sealed class BodyState
{
    /// <summary>
    /// Current location name of its body
    /// </summary>
    public string? LocationName { get; set; }

    /// <summary>
    /// Current location tile of its body
    /// </summary>
    public Vector2 Tile { get; set; }

    /// <summary>
    /// Determines if the body can act
    /// </summary>
    public bool CanAct { get; set; }

    /// <summary>
    /// Determines if the body is in an event
    /// </summary>
    public bool InEvent { get; set; }

    /// <summary>
    /// Determines the current health of our body
    /// </summary>
    public int Health { get; set; }

    /// <summary>
    /// Determines the current energy level of our body
    /// </summary>
    public float Energy { get; set; }
}