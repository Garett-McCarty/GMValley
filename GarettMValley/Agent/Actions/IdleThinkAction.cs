
namespace GarettMValley.Agent.Actions;

public sealed class IdleThinkAction : IAction
{
    public string IntentKey => "idle_think";
    public bool IsFinished { get; private set; } = false;
    public float Score(Blackboard blackboard)
    {
        if (blackboard.Emotion.Arousal < 0.2f && blackboard.Emotion.Stress < 0.3f)
            return 0.3f;
        return 0.0f;
    }
    public bool CanContinue(Blackboard blackboard) => false;
    public void Start(Blackboard blackboard) { 
        blackboard.Emotion.Arousal -= 0.02f;
        blackboard.Emotion.Stress -= 0.01f;
        blackboard.Emotion.Curiosity += 0.01f;
        IsFinished = true; 
    }
    public void Tick(Blackboard blackboard) {}
    public void Abort(Blackboard blackboard) { IsFinished = true; }
}