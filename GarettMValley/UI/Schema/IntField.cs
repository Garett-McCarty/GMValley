namespace GarettMValley.UI.Schema;

internal sealed class IntField : ConfigField
{
    public Func<int> Get { get; }
    public Action<int> Set { get; }
    public int Min { get; }
    public int Max { get; }

    public IntField(string id, string label, Func<int> get, Action<int> set, int min, int max, string? tooltip = null) : base(id, label, tooltip)
    {
        Get = get;
        Set = set;
        Min = min;
        Max = max;
    }

    public string? Validate(string raw)
    {
        if (!int.TryParse(raw, out int value))
            return "Must be a number";
        if (value < Min || value > Max)
            return $"Must be between {Min} and {Max}.";
        return null;
    }
}