using StardewValley;

namespace GarettMValley.AI;

internal static class DialogueContextBuilder
{
    /// <summary>
    /// Build a dialogue context for a given Agent NPC
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="player"></param>
    /// <param name="location"></param>
    /// <param name="currentAction"></param>
    /// <param name="currentIntent"></param>
    /// <param name="emotionSummary"></param>
    /// <param name="personalitySummary"></param>
    /// <returns></returns>
    public static DialogueContext BuildForAgentNPC(NPC npc, Farmer player, GameLocation location, string? currentAction, string? currentIntent, string emotionSummary, string personalitySummary)
    {
        int hearts = 0;
        if (npc != null && !string.IsNullOrEmpty(npc.Name) && player?.friendshipData != null && player.friendshipData.TryGetValue(npc.Name, out var f))
            hearts = f.Points / 250;
        
        var npcName = npc?.Name ?? "NPC";
        return new DialogueContext(
            AgentId: $"npc:{npcName}",
            NpcName: npcName,
            LocationName: location?.NameOrUniqueName ?? "Unknown",
            TimeOfDay: Game1.timeOfDay,
            Season: Game1.currentSeason,
            DayOfMonth: Game1.dayOfMonth,
            FriendshipHearts: hearts,
            PlayerHasActiveObject: player?.ActiveObject is not null,
            IsRaining: Game1.isRaining,
            CurrentAction: currentAction,
            CurrentIntent: currentIntent,
            EmotionSummary: emotionSummary,
            PersonalitySummary: personalitySummary
        );
    }
}