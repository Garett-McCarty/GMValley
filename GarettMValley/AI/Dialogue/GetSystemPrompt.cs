using System.Text;
using GarettMValley.AI.Mind;
using StardewModdingAPI;
using StardewValley;

namespace GarettMValley.AI;

public sealed partial class AiManager
{
    public string GetSystemPromptForNPC(NPC npc)
    {
        Blackboard? blackboard = null;
        string? action = null;
        string? intent = null;
        TryGetRuntimeForNpc(npc, out blackboard, out action, out intent);
        PersonalityProfile personalityProfile = blackboard?.Personality ?? new PersonalityProfile { Key = "default" };
        EmotionalState emotionalState = blackboard?.Emotion ?? EmotionalState.Neutral;
        string personalitySummary = $"Key={personalityProfile.Key}; Sociability={personalityProfile.Sociability:0.00}; Bravery={personalityProfile.Bravery:0.00}; Curiosity={personalityProfile.Curiosity:0.00}; Neuroticism={personalityProfile.Neuroticism:0.00}; Dominance={personalityProfile.Dominance:0.00}";
        string emotionSummary = $"Valence={emotionalState.Valence:0.00}; Arousal={emotionalState.Arousal:0.00}; Dominance={emotionalState.Dominance:0.00}; Stress={emotionalState.Stress:0.00}; SocialNeed={emotionalState.SocialNeed:0.00}; Curiosity={emotionalState.Curiosity:0.00}; Fatigue={emotionalState.Fatigue:0.00}";
        string characterTemplate = this._personalities.GetPromptForCharacterName(npc.Name);

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("You are an NPC in Stardew Valley. Stay strictly in-universe.");
        stringBuilder.AppendLine("Never mention being an AI, a model, a mod, prompts, system messages, or the player being real.");
        stringBuilder.AppendLine("Keep outputs short (1-2 sentences). No emojis. No profanity.");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"NPC_NAME: {npc.Name}");
        if (!string.IsNullOrWhiteSpace(characterTemplate))
        {
            stringBuilder.AppendLine("CHARACTER_TEMPLATE:");
            stringBuilder.AppendLine(characterTemplate.Trim());
            stringBuilder.AppendLine();
        }
        stringBuilder.AppendLine("CURRENT_INTERNALS:");
        stringBuilder.AppendLine($"PERSONALITY: {personalitySummary}");
        stringBuilder.AppendLine($"EMOTION: {emotionSummary}");
        stringBuilder.AppendLine();

        // Contract: JSON only
        stringBuilder.AppendLine("OUTPUT_FORMAT: Return ONLY minified JSON with this schema:");
        stringBuilder.AppendLine("{\"npc_line\":\"string\",\"player_options\":[\"string\",\"string\",\"string\"],\"intent\":\"string\",\"tags\":[\"string\",\"string\"]}");
        stringBuilder.AppendLine("Rules:");
        stringBuilder.AppendLine("- npc_line: what the NPC says now.");
        stringBuilder.AppendLine("- player_options: exactly 3 short replies the player could choose.");
        stringBuilder.AppendLine("- intent: 1 short key like 'smalltalk', 'greeting', 'decline', 'ask_help'.");
        stringBuilder.AppendLine("- tags: 0-2 tags like 'friendly', 'anxious', 'busy', 'flirty', 'angry'.");
        stringBuilder.AppendLine("- Do not include extra keys. Do not include markdown.");

        return stringBuilder.ToString();
    }
}