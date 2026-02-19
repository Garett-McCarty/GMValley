using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using GarettMValley;
using GarettMValley.AI;

namespace GarettMValley.AI.Actions;

public sealed class SpeakAction : IAction
{
    public string IntentKey => "speak";
    public bool IsFinished { get; private set; } = false;
    public float Score(Blackboard blackboard)
    {
        if (!blackboard.PlayerIsNear)
            return 0.0f;
        if (!blackboard.IsOnCooldown("speak"))
            return 0.0f;
        return 0.75f;
    }
    public bool CanContinue(Blackboard _blackboard) => true;
    public void Start(Blackboard _blackboard) { IsFinished = false; }
    public void Tick(Blackboard blackboard)
    { 
        blackboard.Self!.Say("This is speech from SpeakAction!");
        blackboard.SetCooldown("speak", 600);
        IsFinished = true;
    }
    public void Abort(Blackboard _blackboard) { IsFinished = true; }
}