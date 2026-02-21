
namespace GarettMValley.Agent.Actions;

/// <summary>
/// Face target direction action for agents
/// </summary>
public sealed class FaceTargetAction : IAction
{
    public string IntentKey => "face_target";
    public bool IsFinished { get; private set; } = false;
    public float Score(Blackboard blackboard) => blackboard.PlayerIsNear && !blackboard.IsOnCooldown(IntentKey) ? 0.35f : 0.0f;
    public bool CanContinue(Blackboard blackboard) => true;
    public void Start(Blackboard blackboard) { IsFinished = false; }
    public void Tick(Blackboard blackboard) { 
        blackboard.Self!.FaceTile(blackboard.Player!.Tile);
        blackboard.SetCooldown(IntentKey, 20);
        IsFinished = true; 
    }
    public void Abort(Blackboard blackboard) { IsFinished = true; }
}