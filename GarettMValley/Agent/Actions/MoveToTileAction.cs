using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using GarettMValley;
using GarettMValley.Agent;
using Microsoft.Xna.Framework;
using GarettMValley.Agent.Core;
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
    public float Score(MindState mindstate)
    {
        if (mindstate.MoveTargetTile is null)
            return 0.0f;
        if (!Game1.IsMasterGame)
            return 0.0f;
        return 0.9f;
    }
    public bool CanContinue(MindState mindstate) => mindstate.MoveTargetTile is not null;
    public void Start(MindState mindstate) {
        IsFinished = false;
        stuckTicks = 0;
        targetTile = mindstate.MoveTargetTile!.Value;
        lastTile = mindstate.Self!.Tile;
        var character = mindstate.Self.Character;
        var location = mindstate.Location;
        previousController = character.controller;
        character.controller = (StardewValley.Pathfinding.PathFindController)PathControllerFactory.CreatePathFindController(character, location!, targetTile, finalFacingDirection: -1);
        mindstate.StartIntent(IntentKey, ticks: 300); // ~5s
    }
    public void Tick(MindState mindstate)
    {
        var character = mindstate.Self!.Character;
        if (character.controller is null)
        {
            IsFinished = true;
            mindstate.ClearMoveTarget();
            mindstate.ClearIntent();
            return;
        }

        if (Vector2.Distance(mindstate.Self.Tile, targetTile) <= 0.1f)
        {
            character.controller = null;
            character.Halt();
            IsFinished = true;
            mindstate.ClearMoveTarget();
            mindstate.ClearIntent();
            return;
        }

        if (Vector2.Distance(mindstate.Self.Tile, lastTile) < 0.01f)
            stuckTicks += 1;
        else
            stuckTicks = 0;
        lastTile = mindstate.Self.Tile;
        if (stuckTicks >= 4)
        {
            Abort(mindstate);
        }
    }
    public void Abort(MindState mindstate)
    {
        var character = mindstate.Self!.Character;
        character.controller = previousController as StardewValley.Pathfinding.PathFindController;
        character.Halt();
        previousController = null;
        IsFinished = true;
        mindstate.ClearMoveTarget();
        mindstate.ClearIntent();
    }
}