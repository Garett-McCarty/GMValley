
namespace GarettMValley.Agent.Memory;

/// <summary>
/// Record of an episodic memory event
/// </summary>
/// <param name="DateTime">Time of the memory event</param>
/// <param name="Kind">Kind identifier to help categorize the event</param>
/// <param name="Tag">Tag identifier to help categorize the event</param>
/// <param name="TargetId">Target of what the memory event was about</param>
/// <param name="ValenceDelta">Valence potential recorded during event</param>
/// <param name="ArousalDelta">Arousal potential recorded during event</param>
/// <param name="StressDelta">Stress potential recorded during event</param>
internal readonly record struct Episode(
    DateTime DateTime,
    string Kind,
    string? Tag = null,
    long? TargetId = null,
    float ValenceDelta = 0.0f,
    float ArousalDelta = 0.0f,
    float StressDelta = 0.0f
);