namespace GarettMValley.UI.Schema;

internal sealed class BoolField : ConfigField
{
    public Func<bool> Get { get; }
    public Action<bool> Set { get; }

    public BoolField(string id, string label, Func<bool> get, Action<bool> set, string? tooltip = null) : base(id, label, tooltip)
    {
        Get = get;
        Set = set;
    }
}