using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using GarettMValley;
using GarettMValley.AI;

namespace GarettMValley.AI;

public interface IAction
{
    string IntentKey { get; }
    bool IsFinished { get; }
    float Score(Blackboard blackboard);
    bool CanContinue(Blackboard blackboard);

    void Start(Blackboard blackboard);
    void Tick(Blackboard blackboard);
    void Abort(Blackboard blackboard);
}