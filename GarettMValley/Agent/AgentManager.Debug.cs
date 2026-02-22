
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace GarettMValley.Agent;

/// <summary>
/// Debug related portion of the AgentManager
/// </summary>
public sealed partial class AgentManager
{
    /// <summary>
    /// Debug AI information
    /// </summary>
    private bool _debug = true;

    /// <summary>
    /// Print about once/sec
    /// </summary>
    private uint DebugEveryTicks = 60;

    /// <summary>
    /// Debug text for debug mode
    /// </summary>
    private readonly Dictionary<string, string> _debugAgentText = new();

    /// <summary>
    /// Enable or Disable the agent debug mode
    /// </summary>
    /// <param name="enabled"></param>
    public void SetDebugEnabled(bool enabled) => _debug = enabled;

    /// <summary>
    /// Toggle the agent debug mode
    /// </summary>
    public void ToggleDebugEnabled() => _debug = !_debug;

    /// <summary>
    /// Determine if agent debug mode is enabled.
    /// </summary>
    /// <returns></returns>
    public bool GetDebugEnabled() => _debug;

    /// <summary>
    /// Draw a label over a given agent
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="adapter"></param>
    /// <param name="text"></param>
    private void DrawAgentLabel(SpriteBatch spriteBatch, IAgentAdapter adapter, string text)
    {
        if (adapter.Raw is not StardewValley.Character character)
            return;

        // don't draw our debug info if the character is offscreen
        Rectangle viewportRect = new Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height);
        if (!viewportRect.Intersects(character.GetBoundingBox()))
            return;

        // World position (center top of character bounding box)
        var boundingBox = character.GetBoundingBox();

        Vector2 worldPosition = new Vector2(
            boundingBox.Center.X,
            boundingBox.Top - 32.0f // slightly above head
        );

        var lines = text.Count(c => c == '\n') + 1;
        worldPosition.Y -= (lines - 1) * 10.0f;

        // Convert world → screen
        Vector2 screenPosition = Game1.GlobalToLocal(Game1.viewport, worldPosition);

        // Center text
        var size = Game1.smallFont.MeasureString(text);
        screenPosition.X -= size.X / 2f;

        // Draw shadow for readability
        spriteBatch.DrawString(Game1.smallFont, text, screenPosition + new Vector2(1, 1), Color.Black * 0.75f);
        spriteBatch.DrawString(Game1.smallFont, text, screenPosition, Color.Yellow);
    }
}