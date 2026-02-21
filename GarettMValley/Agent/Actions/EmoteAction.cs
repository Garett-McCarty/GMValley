using GarettMValley.Agent.Emote;

namespace GarettMValley.Agent.Actions;

/// <summary>
/// Emote action for agents
/// </summary>
public sealed class EmoteAction : IAction
{
    public string IntentKey => "emote";
    public bool IsFinished { get; private set; } = false;
    public float Score(Blackboard blackboard) => blackboard.PlayerIsNear && !blackboard.IsOnCooldown("emote") ? 0.35f : 0.0f;
    public bool CanContinue(Blackboard _blackboard) => true;
    public void Start(Blackboard _blackboard) { IsFinished = false; }
    public void Tick(Blackboard blackboard)
    { 
        int emote = EmotePicker.Pick(blackboard.Emotion, blackboard.ThreatNearby, EmoteContext.Idle);
        blackboard.Self!.Emote(emote);
        blackboard.SetCooldown(IntentKey, 120); // ~2 seconds if tick interval is fast,
        IsFinished = true;
    }
    public void Abort(Blackboard _blackboard) { IsFinished = true; }
}