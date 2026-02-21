using System.Text;
using StardewValley;
using GarettMValley.Dialogue;

namespace GarettMValley.Agent;

public sealed partial class AiManager
{
    public string GetUserPromptFor(NPC npc, Farmer who, GameLocation location, string playerSaid = "")
    {
        Blackboard? blackboard = null;
        string? action = null;
        string? intent = null;

        TryGetRuntimeForNpc(npc, out blackboard, out action, out intent);

        string emotionSummary = blackboard is null ? "unknown" : $"V={blackboard.Emotion.Valence:0.00} A={blackboard.Emotion.Arousal:0.00} D={blackboard.Emotion.Dominance:0.00} S={blackboard.Emotion.Stress:0.00}";
        string socialSummary = blackboard is null ? "unknown" : $"nearbyFriends={blackboard.NearbyAgentsCount} threat={blackboard.ThreatNearby} distToPlayer={blackboard.DistanceToPlayerTiles:0.0}";

        DialogueContext context = DialogueContextBuilder.BuildForAgentNPC(npc, who, location, currentAction: action, currentIntent: intent, emotionSummary: emotionSummary, personalitySummary: blackboard?.Personality?.Key ?? "default");

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("SCENE:");
        stringBuilder.AppendLine($"- Location: {context.LocationName}");
        stringBuilder.AppendLine($"- Time: {context.TimeOfDay}  Season: {context.Season}  Day: {context.DayOfMonth}");
        stringBuilder.AppendLine($"- Weather: {(context.IsRaining ? "raining" : "clear")}");
        stringBuilder.AppendLine($"- Friendship hearts: {context.FriendshipHearts}");
        stringBuilder.AppendLine($"- Player holding item: {(context.PlayerHasActiveObject ? "yes" : "no")}");
        stringBuilder.AppendLine($"- NPC current action: {context.CurrentAction ?? "none"}  intent: {context.CurrentIntent ?? "none"}");
        stringBuilder.AppendLine($"- Sensors: {socialSummary}");
        stringBuilder.AppendLine($"- Emotion: {context.EmotionSummary}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("PLAYER_ACTION:");
        stringBuilder.AppendLine("- The player initiated conversation with the NPC.");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("PLAYER_SAYS:");
        stringBuilder.AppendLine(playerSaid);
        stringBuilder.AppendLine("TASK:");
        stringBuilder.AppendLine("- Produce a natural NPC line + 3 distinct player reply options.");
        stringBuilder.AppendLine("- Keep it consistent with the NPC personality and current emotion.");
        stringBuilder.AppendLine("- Keep it compatible with vanilla Stardew tone.");
        stringBuilder.AppendLine("- Return only minified JSON with schema:");
        stringBuilder.AppendLine("{\"npc_line\":\"string\",\"player_options\":[\"string\",\"string\",\"string\"],\"intent\":\"string\",\"tags\":[\"string\",\"string\"]}");

        return stringBuilder.ToString();
    }
}