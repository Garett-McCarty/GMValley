using Microsoft.Xna.Framework;

namespace GarettMValley.Agent;

/// <summary>
/// AI related utility helpers
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// Get the direction face value from 2 tiles
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns>Face value of the path</returns>
    public static int DirectionToward(Vector2 from, Vector2 to)
    {
        var distance = to - from;
        if (Math.Abs(distance.X) > Math.Abs(distance.Y))
            return distance.X > 0 ? 1 : 3;
        return distance.Y > 0 ? 2 : 0;
    }

    public static Vector2 ClampToOne(Vector2 value)
    {
        float x = value.X == 0 ? 0 : (value.X > 0 ? 1 : -1);
        float y = value.Y == 0 ? 0 : (value.Y > 0 ? 1 : -1);
        return new Vector2(x, y);
    }
}