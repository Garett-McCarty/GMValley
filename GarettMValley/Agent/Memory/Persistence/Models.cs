
namespace GarettMValley.Agent.Memory.Persistence;

/// <summary>
/// JSON-friendly data model for Agent Memory
/// </summary>
internal sealed class AgentMemoryData
{
    public List<Episode> Episodic { get; set; } = new();
    public LongTermData LongTerm { get; set; } = new();
}

/// <summary>
/// JSON-friendly data model for Agent Long Term Memory
/// </summary>
internal sealed class LongTermData
{
    public Dictionary<string, Preference> PreferencesByTag { get; set; } = new();
    public Dictionary<long, Affinity> AffinityByTargetId { get; set; } = new();
    public Dictionary<string, string> Facts { get; set; } = new();

    public float BaselineValence { get; set; }
    public float BaselineArousal { get; set; }
    public float BaselineStress { get; set; }
}