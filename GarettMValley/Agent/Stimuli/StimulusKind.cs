
namespace GarettMValley.Agent.Stimuli;

/// <summary>
/// Different types of stimulus different Agents can react to
/// </summary>
public enum StimulusKind
{
    /// <summary>
    /// Unknown or uncategorized type of stimulus recorded.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Player entity is present based stimulus recorded.
    /// </summary>
    PlayerPresense,

    /// <summary>
    /// Proximity based stimulus recorded.
    /// </summary>
    Proximity,

    /// <summary>
    /// Threat based stimulus recorded.
    /// </summary>
    Threat,

    /// <summary>
    /// Friendly based stimulus recorded.
    /// </summary>
    Friendly,

    /// <summary>
    /// Language based stimulus recorded.
    /// </summary>
    Language,

    /// <summary>
    /// Biological need based stimulus recorded. (Hunger, Thirst, etc)
    /// </summary>
    BiologicalNeed,
    
    /// <summary>
    /// Visual based stimulus recorded.
    /// </summary>
    Visual,
}
