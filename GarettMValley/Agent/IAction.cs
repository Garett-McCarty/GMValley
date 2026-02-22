using GarettMValley.Agent.Core;

namespace GarettMValley.Agent;

public interface IAction
{
    string IntentKey { get; }
    bool IsFinished { get; }
    float Score(MindState mindstate);
    bool CanContinue(MindState mindstate);
    void Start(MindState mindstate);
    void Tick(MindState mindstate);
    void Abort(MindState mindstate);
}