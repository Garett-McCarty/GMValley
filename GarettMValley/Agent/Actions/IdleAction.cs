using GarettMValley.Agent.Mind;

namespace GarettMValley.Agent.Actions;

public sealed class IdleAction : IAction
{
    public string IntentKey => "idle";
    public bool IsFinished { get; private set; } = false;
    public float Score(MindState mindstate) => 0.01f;
    public bool CanContinue(MindState mindstate) => true;
    public void Start(MindState mindstate) { IsFinished = false; }
    public void Tick(MindState mindstate) { IsFinished = true; }
    public void Abort(MindState mindstate) { IsFinished = true; }
}