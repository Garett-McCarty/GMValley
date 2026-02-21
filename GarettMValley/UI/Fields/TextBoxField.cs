using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace GarettMValley.UI.Fields;

internal sealed class TextBoxField
{
    public string Id { get; }
    public string Label { get; }
    public string? Tooltip { get; }
    public TextBox TextBox { get; }
    public ClickableComponent Clickable { get; }
    
     public TextBoxField(string id, string label, string initialText, int x, int y, int width, string? tooltip = null)
    {
        Id = id;
        Label = label;
        Tooltip = tooltip;

        TextBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
        {
            X = x,
            Y = y,
            Width = width,
            Text = initialText ?? ""
        };

        Clickable = new ClickableComponent(new Rectangle(x, y, width, 48), id);
    }

    public bool ContainsPoint(int x, int y) => Clickable.containsPoint(x, y);

    public void Focus()
    {
        TextBox.SelectMe();
        Game1.keyboardDispatcher.Subscriber = TextBox;
    }

    public void UnfocusIfFocused()
    {
        if (Game1.keyboardDispatcher.Subscriber == TextBox)
            Game1.keyboardDispatcher.Subscriber = null;

        TextBox.Selected = false;
    }

    public void Draw(SpriteBatch b) => TextBox.Draw(b);
}