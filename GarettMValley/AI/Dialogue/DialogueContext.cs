
namespace GarettMValley.AI;

/// <summary>
/// DialogueContext for constructing facts without prompts becoming spaghetti.
/// </summary>
/// <param name="AgentId">Agent who is engaging in the dialogue context</param>
/// <param name="NpcName">Agent name</param>
/// <param name="LocationName">Location name dialogue context</param>
/// <param name="TimeOfDay">Time of day dialogue context</param>
/// <param name="Season">Time of the season dialogue context</param>
/// <param name="DayOfMonth">Day of the month dialogue context</param>
/// <param name="FriendshipHearts">How close we are to the player dialogue context</param>
/// <param name="PlayerHasActiveObject">What the player is holding dialogue context</param>
/// <param name="IsRaining">Environment condition dialogue context</param>
/// <param name="CurrentAction">Current action of the agent dialogue context</param>
/// <param name="CurrentIntent">Current intent of the agent dialogue context</param>
/// <param name="EmotionSummary">Current emotion summary of the agent dialogue context</param>
/// <param name="PersonalitySummary">Summary of the agents personality for the dialogue context</param>
public readonly record struct DialogueContext(
    string AgentId,
    string NpcName,
    string LocationName,
    int TimeOfDay,
    string Season,
    int DayOfMonth,
    int FriendshipHearts,
    bool PlayerHasActiveObject,
    bool IsRaining,
    string? CurrentAction,
    string? CurrentIntent,
    string EmotionSummary,
    string PersonalitySummary
);