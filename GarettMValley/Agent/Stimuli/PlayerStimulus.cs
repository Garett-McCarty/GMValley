
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Stimuli;

public sealed record PlayerStimulus
(
    int Tick,
    string LocationName,
    Vector2 Tile,
    float Intensity
) : StimulusBase(StimulusKind.PlayerPresense, Tick, LocationName, Tile, Intensity);