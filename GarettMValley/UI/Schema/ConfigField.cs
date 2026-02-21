namespace GarettMValley.UI.Schema;

internal abstract class ConfigField
{
    public string Id { get; }
    public string Label { get; }
    public string? Tooltip { get; }

    protected ConfigField(string id, string label, string? tooltip)
    {
        Id = id;
        Label = label;
        Tooltip = tooltip;
    }
}