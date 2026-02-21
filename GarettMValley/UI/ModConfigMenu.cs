using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using GarettMValley.UI.Fields;
using GarettMValley.UI.Schema;
using StardewValley.BellsAndWhistles;

namespace GarettMValley.UI;

internal sealed class ModConfigMenu : IClickableMenu
{
    private readonly IMonitor _monitor;
    private readonly IModHelper _helper;

    private readonly Func<ModConfig> _getConfig;
    private readonly Action<ModConfig> _setConfig;
    private readonly Action<ModConfig> _onSaveApplied;

    private readonly ModConfig _working;
    private readonly List<ConfigField> _schema;
    private List<ConfigField> _visible = new();

    private readonly Dictionary<string, ClickableComponent> _boolClickables = new();
    private readonly Dictionary<string, TextBoxField> _textFields = new();
    private readonly Dictionary<string, ClickableComponent> _choiceClickables = new();

    private readonly ClickableComponent _buttonSave;
    private readonly ClickableComponent _buttonCancel;

    private int _scrollY;
    private int _maxScrollY;

    private const int RowHeight = 60;
    private const int HeaderHeight = 52;

    // layout
    private const int Padding = 48;
    private const int LabelWidth = 320;
    private const int DefaultTextWidth = 360;

    private string? _hoverText;

    public ModConfigMenu(
        IMonitor monitor,
        IModHelper helper,
        Func<ModConfig> getConfig,
        Action<ModConfig> setConfig,
        Action<ModConfig> onSaveApplied
    )
        : base(Game1.viewport.Width / 2 - (840 - 840 / 3), Game1.viewport.Height / 2 - (600 - 600 / 3), 840, 600, showUpperRightCloseButton: true)
    {
        _monitor = monitor;
        _helper = helper;
        _getConfig = getConfig;
        _setConfig = setConfig;
        _onSaveApplied = onSaveApplied;
        _working = getConfig().FromSelf();
        _schema = ModConfigSchema.Build(_working);
        _buttonSave = new ClickableComponent(new Rectangle(xPositionOnScreen + width - 280, yPositionOnScreen + height - 92, 112, 64), "save");
        _buttonCancel = new ClickableComponent(new Rectangle(xPositionOnScreen + width - 150, yPositionOnScreen + height - 92, 112, 64), "cancel");

        RebuildVisibleAndLayout();
    }

    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        base.receiveRightClick(x, y, playSound);

        foreach (var field in _visible)
        {
            if (field is not ChoiceField choiceField)
                continue;

            if (_choiceClickables.TryGetValue(choiceField.Id, out var clickable) && clickable.containsPoint(x, y))
            {
                CycleChoice(choiceField, forward: false);
                if (playSound) Game1.playSound("drumkit6");
                return;
            }
        }
    }

    public override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);
        _scrollY -= Math.Sign(direction) * 40;
        _scrollY = Math.Clamp(_scrollY, 0, _maxScrollY);
        RepositionControls();
    }

    public override void receiveKeyPress(Keys key)
    {
        if (Game1.keyboardDispatcher?.Subscriber is TextBox)
        {
            if (key == Keys.Escape)
            {
                UnfocusAllTextboxes();
            }

            return;
        }

        base.receiveKeyPress(key);

        if (Game1.options.doesInputListContain(Game1.options.menuButton, key))
        {
            UnfocusAllTextboxes();
            return;
        }
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);

        if (destroy)
            return;
        if (_buttonSave.containsPoint(x, y))
        {
            if (TrySave())
            {
                Game1.playSound("money");
                UnfocusAllTextboxes();
                exitThisMenuNoSound();
            }
            else
            {
                Game1.playSound("cancel");
            }
            return;
        }

        if (_buttonCancel.containsPoint(x, y))
        {
            Game1.playSound("bigDeSelect");
            UnfocusAllTextboxes();
            exitThisMenuNoSound();
            return;
        }

        foreach (var textField in _textFields.Values)
        {
            if (textField.ContainsPoint(x, y))
            {
                foreach (var other in _textFields.Values)
                    if (!ReferenceEquals(other, textField))
                        other.UnfocusIfFocused();

                textField.Focus();
                Game1.playSound("shwip");
                return;
            }
        }

        foreach (var field in _visible)
        {
            if (field is not BoolField boolField)
                continue;

            if (_boolClickables.TryGetValue(boolField.Id, out var clickable) && clickable.containsPoint(x, y))
            {
                boolField.Set(!boolField.Get());
                Game1.playSound("drumkit6");

                if (boolField.Id is "EnableOllama" or "EnableHttpApi")
                    RebuildVisibleAndLayout();

                return;
            }
        }

        foreach (var field in _visible)
        {
            if (field is not ChoiceField choiceField)
                continue;

            if (_choiceClickables.TryGetValue(choiceField.Id, out var choiceClickable) && choiceClickable.containsPoint(x, y))
            {
                CycleChoice(choiceField, forward: true);
                Game1.playSound("drumkit6");
                return;
            }
        }
        UnfocusAllTextboxes();
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        _hoverText = null;

        foreach (var field in _visible)
        {
            if (string.IsNullOrWhiteSpace(field.Tooltip))
                continue;
            if (field is BoolField && _boolClickables.TryGetValue(field.Id, out var clickable) && clickable.containsPoint(x, y))
            {
                _hoverText = field.Tooltip;
                return;
            }

            if ((field is StringField or IntField) && _textFields.TryGetValue(field.Id, out var textField) && textField.ContainsPoint(x, y))
            {
                _hoverText = field.Tooltip;
                return;
            }
        }
    }

    public override void update(GameTime time)
    {
        base.update(time);
    }

    public override void draw(SpriteBatch batch)
    {
        batch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
        IClickableMenu.drawTextureBox(batch, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
        SpriteText.drawString(batch, "GMValley Config", xPositionOnScreen + Padding, yPositionOnScreen + 20);

        int listX = xPositionOnScreen + Padding;
        int listY = yPositionOnScreen + 76;
        int listW = width - Padding * 2;
        int listH = height - 76 - 120;

        int yCursor = listY - _scrollY;

        foreach (var field in _visible)
        {
            if (field is HeaderField header)
            {
                if (yCursor + HeaderHeight >= listY && yCursor <= listY + listH)
                {
                    batch.DrawString(Game1.smallFont, header.Label, new Vector2(listX, yCursor + 8), Game1.textColor);
                    batch.Draw(Game1.fadeToBlackRect, new Rectangle(listX, yCursor + 36, listW, 2), Color.Black * 0.25f);
                }
                yCursor += HeaderHeight;
                continue;
            }

            if (yCursor + RowHeight < listY)
            {
                yCursor += RowHeight;
                continue;
            }
            if (yCursor > listY + listH)
            {
                yCursor += RowHeight;
                continue;
            }

            batch.DrawString(Game1.smallFont, field.Label, new Vector2(listX, yCursor + 14), Game1.textColor);

            int fieldX = listX + LabelWidth;
            int fieldY = yCursor + 6;

            if (field is BoolField bf)
            {
                DrawCheckbox(batch, fieldX, fieldY, bf.Get());
            }
            else if (field is StringField or IntField)
            {
                if (_textFields.TryGetValue(field.Id, out var textField))
                    textField.Draw(batch);
            }
            else if (field is ChoiceField choiceField)
            {
                DrawChoiceBox(batch, fieldX, fieldY, DefaultTextWidth, choiceField.Get());
            }

            yCursor += RowHeight;
        }

        DrawButton(batch, _buttonSave.bounds, "Save");
        DrawButton(batch, _buttonCancel.bounds, "Cancel");

        if (!string.IsNullOrWhiteSpace(_hoverText))
            IClickableMenu.drawHoverText(batch, _hoverText, Game1.smallFont);

        base.draw(batch);
        drawMouse(batch);
    }

    private void RebuildVisibleAndLayout()
    {
        bool ollamaOn = _schema.OfType<BoolField>().FirstOrDefault(x => x.Id == "EnableOllama")?.Get() ?? false;
        bool httpOn = _schema.OfType<BoolField>().FirstOrDefault(x => x.Id == "EnableHttpApi")?.Get() ?? false;
        _visible = new List<ConfigField>();
        foreach (var field in _schema)
        {
            if (field.Id.StartsWith("Ollama", StringComparison.OrdinalIgnoreCase) && !ollamaOn && field is not HeaderField)
                continue;
            if (field.Id.StartsWith("HttpApi", StringComparison.OrdinalIgnoreCase) && !httpOn && field is not HeaderField)
                continue;

            _visible.Add(field);
        }

        BuildControls();
        RepositionControls();
    }

    private void BuildControls()
    {
        _boolClickables.Clear();
        _textFields.Clear();
        _choiceClickables.Clear();

        int listX = xPositionOnScreen + Padding;
        int listY = yPositionOnScreen + 76;
        int yCursor = listY;

        foreach (var field in _visible)
        {
            if (field is HeaderField)
            {
                yCursor += HeaderHeight;
                continue;
            }

            int fieldX = listX + LabelWidth;
            int fieldY = yCursor + 6;

            if (field is BoolField)
            {
                _boolClickables[field.Id] = new ClickableComponent(new Rectangle(fieldX, fieldY, 48, 48), field.Id);
            }
            else if (field is StringField stringField)
            {
                _textFields[field.Id] = new TextBoxField(field.Id, field.Label, stringField.Get() ?? "", fieldX, fieldY, DefaultTextWidth, field.Tooltip);
            }
            else if (field is IntField intField)
            {
                _textFields[field.Id] = new TextBoxField(field.Id, field.Label, intField.Get().ToString(), fieldX, fieldY, 160, field.Tooltip);
            }
            else if (field is ChoiceField choiceField)
            {
                _choiceClickables[field.Id] = new ClickableComponent(new Rectangle(fieldX, fieldY, DefaultTextWidth, 48), field.Id);
            }

            yCursor += RowHeight;
        }

        int totalHeight = _visible.Sum(f => f is HeaderField ? HeaderHeight : RowHeight);
        int listH = height - 76 - 120;
        _maxScrollY = Math.Max(0, totalHeight - listH);
        _scrollY = Math.Clamp(_scrollY, 0, _maxScrollY);
    }

    private void RepositionControls()
    {
        int listX = xPositionOnScreen + Padding;
        int listY = yPositionOnScreen + 76;
        int yCursor = listY - _scrollY;

        foreach (var field in _visible)
        {
            if (field is HeaderField)
            {
                yCursor += HeaderHeight;
                continue;
            }

            int fieldX = listX + LabelWidth;
            int fieldY = yCursor + 6;

            if (field is BoolField && _boolClickables.TryGetValue(field.Id, out var boolClickable))
            {
                boolClickable.bounds = new Rectangle(fieldX, fieldY, 48, 48);
            }
            else if (_textFields.TryGetValue(field.Id, out var textField))
            {
                textField.TextBox.X = fieldX;
                textField.TextBox.Y = fieldY;
                textField.Clickable.bounds = new Rectangle(fieldX, fieldY, textField.Clickable.bounds.Width, textField.Clickable.bounds.Height);
            }
            else if (field is ChoiceField && _choiceClickables.TryGetValue(field.Id, out var choiceClickable))
            {
                choiceClickable.bounds = new Rectangle(fieldX, fieldY, DefaultTextWidth, 48);
            }

            yCursor += RowHeight;
        }
    }

    private bool TrySave()
    {
        foreach (var field in _visible)
        {
            if (field is StringField stringField && _textFields.TryGetValue(field.Id, out var stringTextField))
            {
                var raw = stringTextField.TextBox.Text ?? "";
                var err = stringField.Validate?.Invoke(raw);
                if (err != null)
                {
                    Game1.showRedMessage($"{stringField.Label}: {err}");
                    return false;
                }
            }
            else if (field is IntField intField && _textFields.TryGetValue(field.Id, out var intTextField))
            {
                var raw = intTextField.TextBox.Text ?? "";
                var err = intField.Validate(raw);
                if (err != null)
                {
                    Game1.showRedMessage($"{intField.Label}: {err}");
                    return false;
                }
            }
        }

        foreach (var field in _schema)
        {
            if (field is StringField stringField && _textFields.TryGetValue(field.Id, out var stringTextField))
            {
                stringField.Set(stringTextField.TextBox.Text ?? "");
            }
            else if (field is IntField intField && _textFields.TryGetValue(field.Id, out var intTextField))
            {
                if (int.TryParse(intTextField.TextBox.Text, out int value))
                {
                    value = Math.Clamp(value, intField.Min, intField.Max);
                    intField.Set(value);
                }
            }
        }

        try
        {
            _setConfig(_working.FromSelf());
            _helper.WriteConfig(_getConfig());
            _onSaveApplied(_getConfig());
            return true;
        }
        catch (Exception ex)
        {
            _monitor.Log($"Failed to save config: {ex}", LogLevel.Error);
            Game1.showRedMessage("Failed to save config. See log.");
            return false;
        }
    }

    private void UnfocusAllTextboxes()
    {
        foreach (var textField in _textFields.Values)
            textField.UnfocusIfFocused();

        if (Game1.keyboardDispatcher.Subscriber is TextBox)
            Game1.keyboardDispatcher.Subscriber = null;
    }

    private static void DrawCheckbox(SpriteBatch batch, int x, int y, bool value)
    {
        IClickableMenu.drawTextureBox(batch, x, y, 48, 48, Color.White);
        if (value)
            batch.DrawString(Game1.smallFont, "X", new Vector2(x + 14, y + 8), Game1.textColor);
    }

    private static void DrawButton(SpriteBatch batch, Rectangle rectangle, string text)
    {
        IClickableMenu.drawTextureBox(batch, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Color.White);
        var size = Game1.smallFont.MeasureString(text);
        batch.DrawString(Game1.smallFont, text,
            new Vector2(rectangle.X + (rectangle.Width - size.X) / 2f, rectangle.Y + (rectangle.Height - size.Y) / 2f + 2f),
            Game1.textColor);
    }

    private static void DrawChoiceBox(SpriteBatch b, int x, int y, int w, string value)
    {
        IClickableMenu.drawTextureBox(b, x, y, w, 48, Color.White);
        b.DrawString(Game1.smallFont, value ?? "", new Vector2(x + 12, y + 12), Game1.textColor);

        // little "↓" hint
        b.DrawString(Game1.smallFont, "↓", new Vector2(x + w - 26, y + 10), Game1.textColor);
    }

    private static void CycleChoice(ChoiceField choiceField, bool forward)
    {
        var current = choiceField.Get() ?? "";
        int idx = -1;

        for (int i = 0; i < choiceField.Options.Count; i++)
        {
            if (string.Equals(choiceField.Options[i], current, StringComparison.OrdinalIgnoreCase))
            {
                idx = i;
                break;
            }
        }

        if (idx < 0)
            idx = 0;
        else
            idx = forward
                ? (idx + 1) % choiceField.Options.Count
                : (idx - 1 + choiceField.Options.Count) % choiceField.Options.Count;

        choiceField.Set(choiceField.Options[idx]);
    }
}