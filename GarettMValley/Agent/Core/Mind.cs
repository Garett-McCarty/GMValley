
using GarettMValley.Agent.Memory;
using GarettMValley.Agent.Mind.Emotion;

namespace GarettMValley.Agent.Core;

internal sealed class Mind
{
    public MindState mindState { get; }
    public EmotionalState emotionalState { get; }
    public MemoryStore store { get; }

    public Mind(MindState mindState, EmotionalState emotionalState, MemoryStore store)
    {
        this.mindState = mindState;
        this.emotionalState = emotionalState;
        this.store = store;
    }

    public void Tick(MindTickContext context)
    {
    }
}

internal readonly record struct MindTickContext(DateTime DateTime, string? LocationName, int TimeOfDay);