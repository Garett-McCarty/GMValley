using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using GarettMValley;
using GarettMValley.Agent;
using Microsoft.Xna.Framework;
using GarettMValley.Agent.Pathing;

namespace GarettMValley.Agent.Actions;

public sealed class MoveToTileAction : IAction
{
    public string IntentKey => "move_to";

    private Vector2 targetTile;
    private int stuckTicks;
    private Vector2 lastTile;
    private object? previousController;

    public bool IsFinished { get; private set; } = false;
    public float Score(Blackboard blackboard)
    {
        if (blackboard.MoveTargetTile is null)
            return 0.0f;
        if (!Game1.IsMasterGame)
            return 0.0f;
        return 0.9f;
    }
    public bool CanContinue(Blackboard blackboard) => blackboard.MoveTargetTile is not null;
    public void Start(Blackboard blackboard) {
        IsFinished = false;
        stuckTicks = 0;
        targetTile = blackboard.MoveTargetTile!.Value;
        lastTile = blackboard.Self!.Tile;
        var character = blackboard.Self.Character;
        var location = blackboard.Location;
        previousController = character.controller;
        character.controller = (StardewValley.Pathfinding.PathFindController)PathControllerFactory.CreatePathFindController(character, location!, targetTile, finalFacingDirection: -1);
        blackboard.StartIntent(IntentKey, ticks: 300); // ~5s
    }
    public void Tick(Blackboard blackboard)
    {
        var character = blackboard.Self!.Character;
        if (character.controller is null)
        {
            IsFinished = true;
            blackboard.ClearMoveTarget();
            blackboard.ClearIntent();
            return;
        }

        if (Vector2.Distance(blackboard.Self.Tile, targetTile) <= 0.1f)
        {
            character.controller = null;
            character.Halt();
            IsFinished = true;
            blackboard.ClearMoveTarget();
            blackboard.ClearIntent();
            return;
        }

        if (Vector2.Distance(blackboard.Self.Tile, lastTile) < 0.01f)
            stuckTicks += 1;
        else
            stuckTicks = 0;
        lastTile = blackboard.Self.Tile;
        if (stuckTicks >= 4)
        {
            Abort(blackboard);
        }
    }
    public void Abort(Blackboard blackboard)
    {
        var character = blackboard.Self!.Character;
        character.controller = previousController as StardewValley.Pathfinding.PathFindController;
        character.Halt();
        previousController = null;
        IsFinished = true;
        blackboard.ClearMoveTarget();
        blackboard.ClearIntent();
    }
}