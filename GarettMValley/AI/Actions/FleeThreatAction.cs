using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using GarettMValley;
using GarettMValley.AI;
using System.Net;

namespace GarettMValley.AI.Actions;

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
        var target = blackboard.Self.Tile + AI.Utils.ClampToOne(away);
        blackboard.Self.TryMoveToward(target);
        _steps += 1;
        if (_steps == 1 && !blackboard.IsOnCooldown("flee_emote"))
        {
            blackboard.Self.Emote(12);
            blackboard.SetCooldown("flee_emote", 180);
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