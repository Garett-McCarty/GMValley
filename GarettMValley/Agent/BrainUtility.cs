using GarettMValley.Agent.Core;

namespace GarettMValley.Agent;

public sealed class BrainUtility
{
    private readonly List<IAction> _actions = new()
    {
        new Actions.EmoteAction(),
        new Actions.FaceTargetAction(),
        new Actions.FleeThreatAction(),
        new Actions.GreetNpcAction(),
        new Actions.GreetPlayerAction(),
        new Actions.IdleAction(),
        new Actions.MoveToTileAction(),
        new Actions.SpeakAction(),
    };

    public IAction ChooseAction(MindState mindstate)
    {
        IAction bestAction = _actions[0];
        float bestScore = float.NegativeInfinity;
        foreach (var action in _actions)
        {
            var score = action.Score(mindstate);
            if (score > bestScore)
            {
                bestAction = action;
                bestScore = score;
            }
        }

        return bestAction;
    }
}