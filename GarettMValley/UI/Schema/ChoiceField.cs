namespace GarettMValley.UI.Schema;

internal sealed class ChoiceField : ConfigField
{
    public Func<string> Get { get; }
    public Action<string> Set { get; }
    public IReadOnlyList<string> Options { get; }
    public bool AllowCustom { get; }
    public ChoiceField(string id, string label, Func<string> get, Action<string> set, IReadOnlyList<string> options, bool allowCustom = false, string? tooltip = null) : base(id, label, tooltip)
    {
        Get = get;
        Set = set;
        Options = options;
        AllowCustom = allowCustom;
    }
}