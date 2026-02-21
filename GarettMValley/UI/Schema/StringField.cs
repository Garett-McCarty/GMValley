namespace GarettMValley.UI.Schema;

internal sealed class StringField : ConfigField
{
    public Func<string> Get { get; }
    public Action<string> Set { get; }
    public Func<string, string?>? Validate { get; }
    
    public StringField(string id, string label, Func<string> get, Action<string> set, Func<string, string?>? validate = null, string? tooltip = null) : base(id, label, tooltip)
    {
        Get = get;
        Set = set;
        Validate = validate;
    }
}