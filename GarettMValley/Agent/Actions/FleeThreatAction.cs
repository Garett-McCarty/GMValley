using GarettMValley.Agent.Core;
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
    public float Score(MindState mindstate) => mindstate.ThreatNearby ? 1.5f : 0.0f;
    public bool CanContinue(MindState mindstate) => mindstate.ThreatNearby && _steps < 6;
    public void Start(MindState mindstate) { IsFinished = false; _steps = 0; }
    public void Tick(MindState mindstate)
    {
        var away = mindstate.Self!.Tile - mindstate.ThreatTile;
        var target = mindstate.Self.Tile + Agent.MathUtils.ClampToOne(away);
        mindstate.Self.TryMoveToward(target);
        _steps += 1;
        if (_steps == 1 && !mindstate.IsOnCooldown(IntentKey))
        {
            int emote = EmotePicker.Pick(mindstate.Emotion, threatNearby: true, EmoteContext.Threat);
            mindstate.Self.Emote(emote);
            mindstate.SetCooldown(IntentKey, 180);
        }

        if (_steps >= 6)
            IsFinished = true;
    }

    public void Abort(MindState mindstate)
    {
        mindstate.Self!.StopMoving();
        IsFinished = true;
    }
}