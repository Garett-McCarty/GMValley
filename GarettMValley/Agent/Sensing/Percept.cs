
namespace GarettMValley.Agent.Perception;

public enum PerceptType
{
    NearbyFriend,
    NearbyThreat,
    Hungry,
    PlayerNearby,
    Raining,
    WorkTime,
}

/// <summary>
/// Record of a perception event
/// </summary>
/// <param name="PerceptType"></param>
/// <param name="Intensity"></param>
/// <param name="Tag"></param>
/// <param name="TargetId"></param>
public readonly record struct Percept(PerceptType PerceptType, float Intensity, string? Tag = null, string? TargetId = null);
