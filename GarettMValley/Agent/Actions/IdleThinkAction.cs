using GarettMValley.Agent.Core;

namespace GarettMValley.Agent.Actions;

public sealed class IdleThinkAction : IAction
{
    public string IntentKey => "idle_think";
    public bool IsFinished { get; private set; } = false;
    public float Score(MindState mindstate)
    {
        if (mindstate.Emotion.Arousal < 0.2f && mindstate.Emotion.Stress < 0.3f)
            return 0.3f;
        return 0.0f;
    }
    public bool CanContinue(MindState _mindstate) => false;
    public void Start(MindState mindstate) { 
        mindstate.Emotion.Arousal -= 0.02f;
        mindstate.Emotion.Stress -= 0.01f;
        mindstate.Emotion.Curiosity += 0.01f;
        IsFinished = true; 
    }
    public void Tick(MindState _mindstate) {}
    public void Abort(MindState _mindstate) { IsFinished = true; }
}