using GarettMValley.Agent.Emote;

namespace GarettMValley.Agent.Actions;

/// <summary>
/// Flee threat action for agents
/// </summary>
public sealed class FleeThreatAction : IAction
{
    public string IntentKey => "flee_threat";
    private int _steps;

    public bool IsFinished { get; private set; } = false;
    public float Score(Blackboard blackboard) => blackboard.ThreatNearby ? 1.5f : 0.0f;
    public bool CanContinue(Blackboard blackboard) => blackboard.ThreatNearby && _steps < 6;
    public void Start(Blackboard blackboard) { IsFinished = false; _steps = 0; }
    public void Tick(Blackboard blackboard)
    {
        var away = blackboard.Self!.Tile - blackboard.ThreatTile;
        var target = blackboard.Self.Tile + Agent.MathUtils.ClampToOne(away);
        blackboard.Self.TryMoveToward(target);
        _steps += 1;
        if (_steps == 1 && !blackboard.IsOnCooldown(IntentKey))
        {
            int emote = EmotePicker.Pick(blackboard.Emotion, threatNearby: true, EmoteContext.Threat);
            blackboard.Self.Emote(emote);
            blackboard.SetCooldown(IntentKey, 180);
        }

        if (_steps >= 6)
            IsFinished = true;
    }

    public void Abort(Blackboard blackboard)
    {
        blackboard.Self!.StopMoving();
        IsFinished = true;
    }
}