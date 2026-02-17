using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using GarettMValley;
using GarettMValley.AI;

namespace GarettMValley.AI.Actions;

public sealed class FaceTargetAction : IAction
{
    public string IntentKey => "face_target";
    public bool IsFinished { get; private set; } = false;
    public float Score(Blackboard blackboard) => blackboard.PlayerIsNear && !blackboard.IsOnCooldown("face_player") ? 0.35f : 0.0f;
    public bool CanContinue(Blackboard blackboard) => true;
    public void Start(Blackboard blackboard) { IsFinished = false; }
    public void Tick(Blackboard blackboard) { 
        blackboard.Self.FaceTile(blackboard.Player.Tile);
        blackboard.SetCooldown("face_player", 20);
        IsFinished = true; 
    }
    public void Abort(Blackboard blackboard) { IsFinished = true; }
}