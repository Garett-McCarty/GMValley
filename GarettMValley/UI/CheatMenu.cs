using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using GarettMValley.Cheats;

namespace GarettMValley.UI;

/// <summary>
/// Simple cheat menu for executing registered cheats.
/// </summary>
internal sealed class CheatMenu : IClickableMenu
{
    private readonly IMonitor _log;
    private readonly CheatManager _cheats;
    private readonly Func<CheatContext> _getContext;

    private readonly List<ICheat> _list;
    private int _selectedIndex;
    private int _scroll;

    private readonly TextBox _argsBox;
    private readonly ClickableComponent _runButton;
    private readonly ClickableComponent _closeButton;

    private const int RowHeight = 44;
    private const int VisibleRows = 10;

    public CheatMenu(IMonitor log, CheatManager cheats, Func<CheatContext> getContext)
        : base(Game1.viewport.Width / 2 - 360, Game1.viewport.Height / 2 - 320, 720, 640, showUpperRightCloseButton: true)
    {
        _log = log;
        _cheats = cheats;
        _getContext = getContext;
        _list = cheats.All.ToList();

        _argsBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
        {
            X = xPositionOnScreen + 32,
            Y = yPositionOnScreen + height - 140,
            Width = width - 64,
            Text = ""
        };

        _runButton = new ClickableComponent(new Rectangle(xPositionOnScreen + width - 160, yPositionOnScreen + height - 92, 128, 44), "run")
        {
            name = "Run"
        };

        _closeButton = upperRightCloseButton;
    }

    public override void receiveKeyPress(Microsoft.Xna.Framework.Input.Keys key)
    {
        base.receiveKeyPress(key);

        if (key == Microsoft.Xna.Framework.Input.Keys.Escape)
        {
            exitThisMenu();
            return;
        }

        // Let the textbox handle typing.
        if (Game1.keyboardDispatcher?.Subscriber == _argsBox)
            return;
    }

    public override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);
        if (_list.Count <= VisibleRows)
            return;

        if (direction > 0)
            _scroll = Math.Max(0, _scroll - 1);
        else if (direction < 0)
            _scroll = Math.Min(Math.Max(0, _list.Count - VisibleRows), _scroll + 1);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);

        // close
        if (_closeButton is not null && _closeButton.containsPoint(x, y))
        {
            exitThisMenu();
            return;
        }

        // run
        if (_runButton.containsPoint(x, y))
        {
            RunSelected();
            return;
        }

        // list rows
        var listArea = GetListArea();
        if (listArea.Contains(x, y))
        {
            int localY = y - listArea.Y;
            int row = localY / RowHeight;
            int index = _scroll + row;
            if (index >= 0 && index < _list.Count)
            {
                _selectedIndex = index;
                Game1.playSound("shiny4");
            }
        }
    }

    private void RunSelected()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _list.Count)
            return;

        var cheat = _list[_selectedIndex];
        var ctx = _getContext();

        string[] args = SplitArgs(_argsBox.Text);
        if (_cheats.TryRun(cheat.Id, ctx, args, out var err))
        {
            Game1.playSound("coin");
        }
        else
        {
            Game1.playSound("cancel");
            if (!string.IsNullOrWhiteSpace(err))
                _log.Log(err, LogLevel.Warn);
        }
    }

    private static string[] SplitArgs(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<string>();

        return text
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToArray();
    }

    private Rectangle GetListArea()
        => new Rectangle(xPositionOnScreen + 24, yPositionOnScreen + 96, width - 48, RowHeight * VisibleRows);

    public override void draw(SpriteBatch b)
    {
        // dim background
        b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

        // window
        IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
        drawTitle(b);

        // list
        var listArea = GetListArea();
        IClickableMenu.drawTextureBox(b, listArea.X, listArea.Y, listArea.Width, listArea.Height, Color.White);

        for (int i = 0; i < VisibleRows; i++)
        {
            int index = _scroll + i;
            if (index >= _list.Count)
                break;

            var cheat = _list[index];
            var rowRect = new Rectangle(listArea.X + 12, listArea.Y + i * RowHeight + 6, listArea.Width - 24, RowHeight - 12);

            if (index == _selectedIndex)
                b.Draw(Game1.staminaRect, rowRect, Color.White * 0.15f);

            string toggleMark = cheat.IsToggle ? (_cheats.IsEnabled(cheat.Id) ? "[ON] " : "[off] ") : "";
            string label = toggleMark + cheat.Name;
            b.DrawString(Game1.smallFont, label, new Vector2(rowRect.X + 8, rowRect.Y + 6), Game1.textColor);
        }

        // description
        var descRect = new Rectangle(xPositionOnScreen + 24, yPositionOnScreen + 32, width - 48, 56);
        string desc = (_selectedIndex >= 0 && _selectedIndex < _list.Count)
            ? _list[_selectedIndex].Description
            : "Select a cheat, then press Run.";
        b.DrawString(Game1.smallFont, desc, new Vector2(descRect.X + 8, descRect.Y + 8), Game1.textColor);

        // args
        b.DrawString(Game1.smallFont, "Args:", new Vector2(xPositionOnScreen + 32, yPositionOnScreen + height - 170), Game1.textColor);
        _argsBox.Draw(b);

        // run button
        IClickableMenu.drawTextureBox(b, _runButton.bounds.X, _runButton.bounds.Y, _runButton.bounds.Width, _runButton.bounds.Height, Color.White);
        var runText = "Run";
        var runSize = Game1.smallFont.MeasureString(runText);
        b.DrawString(
            Game1.smallFont,
            runText,
            new Vector2(_runButton.bounds.Center.X - runSize.X / 2f, _runButton.bounds.Center.Y - runSize.Y / 2f),
            Game1.textColor
        );

        // close button
        base.draw(b);
        drawMouse(b);
    }

    private void drawTitle(SpriteBatch b)
    {
        var title = "GMValley Cheats";
        var size = Game1.dialogueFont.MeasureString(title);
        b.DrawString(Game1.dialogueFont, title, new Vector2(xPositionOnScreen + width / 2f - size.X / 2f, yPositionOnScreen + 8), Game1.textColor);
    }
}
