using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using GarettMValley;
using GarettMValley.AI;

namespace GarettMValley.AI.Actions;

public sealed class GreetPlayerAction : IAction
{
    public string IntentKey => "greet_player";
    public bool IsFinished { get; private set; } = false;
    public float Score(Blackboard blackboard)
    {
        if (!blackboard.PlayerIsNear) return 0.0f;
        if (!blackboard.IsOnCooldown("greet_player")) return 0.0f;
        if (blackboard.Self.Kind != "villager") return 0.0f;
        return 1.0f;
    }
    public bool CanContinue(Blackboard blackboard) => blackboard.PlayerIsNear;
    public void Start(Blackboard blackboard) { IsFinished = false; blackboard.StartIntent(IntentKey, ticks: 40); }
    public void Tick(Blackboard blackboard)
    {
        blackboard.Self.FaceTile(blackboard.Player.Tile);
        blackboard.Self.Emote(20);
        blackboard.Self.Say("npc.greet.001");
        blackboard.SetCooldown("greet_player", 600);
        IsFinished = true;
    }
    public void Abort(Blackboard blackboard) { IsFinished = true; blackboard.ClearIntent(); }
}