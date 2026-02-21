
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Stimuli;

/// <summary>
/// Represents some kind of stimulus Agents can sense from its environment.
/// </summary>
public interface IStimulus
{
    /// <summary>
    /// Get the stimulation kind
    /// </summary>
    StimulusKind StimulusKind { get; }

    /// <summary>
    /// Get the tick when the stimulation was created.
    /// </summary>
    int Tick { get; }

    /// <summary>
    /// Get the location name of where the stimulation was created.
    /// </summary>
    string? LocationName { get; }

    /// <summary>
    /// Optional location tile of where stimulation was created.
    /// </summary>
    Vector2 Tile { get; }

    /// <summary>
    /// Intensity of the stimulation from range: [0..1]
    /// </summary>
    float Intensity { get; }
}