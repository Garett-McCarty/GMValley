using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace GarettMValley.AI.Pathing;

public static class PathControllerFactory
{
    /// <summary>
    /// Create a PathFinding controller
    /// </summary>
    /// <param name="character"></param>
    /// <param name="location"></param>
    /// <param name="targetTile"></param>
    /// <param name="finalFacingDirection"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static object CreatePathFindController(Character character, GameLocation location, Vector2 targetTile, int finalFacingDirection = -1)
    {
        var end = new Point((int)targetTile.X, (int)targetTile.Y);
        var type = typeof (StardewValley.Pathfinding.PathFindController);
        object? Try(params object[] args)
        {
            try { return Activator.CreateInstance(type, args); }
            catch
            {
                return null;
            }
        }

        return 
            Try(character, location, end, finalFacingDirection) ??
            Try(character, location, end, finalFacingDirection, false) ??
            Try(character, location, end, finalFacingDirection, false, false) ??
            throw new InvalidOperationException("could not construct PathFindController (constructor signature mismatch).");
    }
}