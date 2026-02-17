using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using GarettMValley;
using GarettMValley.AI;

namespace GarettMValley.AI.Actions;

public sealed class IdleAction : IAction
{
    public string IntentKey => "idle";
    public bool IsFinished { get; private set; } = false;
    public float Score(Blackboard blackboard) => 0.01f;
    public bool CanContinue(Blackboard blackboard) => true;
    public void Start(Blackboard blackboard) { IsFinished = false; }
    public void Tick(Blackboard blackboard) { IsFinished = true; }
    public void Abort(Blackboard blackboard) { IsFinished = true; }
}