using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using GarettMValley;
using GarettMValley.AI;
using System.Security.Cryptography.X509Certificates;

namespace GarettMValley.AI.Actions;

public sealed class GreetNpcAction : IAction
{
    public string IntentKey => "greet_npc";
    public bool IsFinished { get; private set; } = false;
    public float Score(Blackboard blackboard)
    {
        if (blackboard.Self!.Kind != "villager") return 0.0f;
        if (blackboard.IsOnCooldown("greet_npc")) return 0.0f;

        if (blackboard.NearestFriendlyId is null) return 0.0f;
        if (blackboard.DistToNearestFriendlyTiles > 3.0) return 0.0f;
        if (blackboard.PlayerIsNear) return 0.15f;
        return 0.75f;
    }
    public bool CanContinue(Blackboard blackboard) => blackboard.NearestFriendlyId is not null && blackboard.DistToNearestFriendlyTiles <= 4.0f;
    public void Start(Blackboard blackboard) { IsFinished = false; blackboard.StartIntent(IntentKey, 30); }
    public void Tick(Blackboard blackboard)
    {
        blackboard.Self!.FaceTile(blackboard.NearestFriendlyTile);
        blackboard.Self.Emote(20);
        blackboard.SetCooldown("greet_npc", 900);
        IsFinished = true;
    }
    public void Abort(Blackboard blackboard) { IsFinished = true; blackboard.ClearIntent(); }
}