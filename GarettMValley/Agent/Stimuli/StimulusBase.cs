
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Stimuli;

public abstract record StimulusBase(StimulusKind StimulusKind, int Tick, string? LocationName, Vector2 Tile, float Intensity): IStimulus;