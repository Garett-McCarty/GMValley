
using GarettMValley.Agent.Core;

namespace GarettMValley.Agent.Actions;

/// <summary>
/// Face target direction action for agents
/// </summary>
public sealed class FaceTargetAction : IAction
{
    /// <summary>
    /// Face target intent key
    /// </summary>
    public string IntentKey => "face_target";

    /// <summary>
    /// Flag to determine if we are currently in this action
    /// </summary>
    public bool IsFinished { get; private set; } = false;
    public float Score(MindState mindstate) => mindstate.PlayerIsNear && !mindstate.IsOnCooldown(IntentKey) ? 0.35f : 0.0f;
    public bool CanContinue(MindState mindstate) => true;
    public void Start(MindState mindstate) { IsFinished = false; }
    public void Tick(MindState mindstate) { 
        mindstate.Self!.FaceTile(mindstate.Player!.Tile);
        mindstate.SetCooldown(IntentKey, 20);
        IsFinished = true; 
    }
    public void Abort(MindState mindstate) { IsFinished = true; }
}