
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Stimuli;

public sealed record VisualStimulus
(
    int Tick,
    string LocationName,
    string TargetId,
    string TargetKind,
    Vector2 Tile,
    float Intensity,
    float Appraisal,
    float Novelty,
    float Threat
) : StimulusBase(StimulusKind.Visual, Tick, LocationName, Tile, Intensity);