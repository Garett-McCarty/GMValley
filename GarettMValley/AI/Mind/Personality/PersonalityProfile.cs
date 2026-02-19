
using Microsoft.Xna.Framework;
using GarettMValley.AI.Mind.Emotion;

namespace GarettMValley.AI.Mind.Personality;

/// <summary>
/// Personality profile for AI Agents
/// </summary>
public sealed class PersonalityProfile
{
    /// <summary>
    /// Unique key for this personality profile
    /// </summary>
    public string Key { get; init; } = "default";

    /// <summary>
    /// Baseline emotional state for this personality
    /// </summary>
    public EmotionalState Baseline { get; init; } = EmotionalState.Neutral;

    /// <summary>
    /// How fast they return to Baseline in seconds
    /// </summary>
    public float EmotionalHalfLife { get; init; } = 18.0f;

    /// <summary>
    /// Agents sociability trait potential in the range: (0.0f, 1.0f)
    /// </summary>
    public float Sociability { get; init; } = 0.5f;

    /// <summary>
    /// Agents bravery trait potential in the range: (0.0f, 1.0f)
    /// </summary>
    public float Bravery { get; init; } = 0.5f;

    /// <summary>
    /// Agents curiosity trait potential in the range: (0.0f, 1.0f)
    /// </summary>
    public float Curiosity { get; init; } = 0.5f;

    /// <summary>
    /// Agents neuroticism trait potential in the range: (0.0f, 1.0f)
    /// </summary>
    public float Neuroticism { get; init; } = 0.5f;

    /// <summary>
    /// Agents dominance trait potential in the range: (0.0f, 1.0f)
    /// </summary>
    public float Dominance { get; init; } = 0.5f;

    /// <summary>
    /// Personality's threat sensitivity multiplier usually in the range: (0.5, 2.0)
    /// </summary>
    public float ThreatSensitivity { get; init; } = 1.0f;

    /// <summary>
    /// Personality's social sensitivity multiplier usually in the range: (0.5, 2.0)
    /// </summary>
    public float SocialSensitivity { get; init; } = 1.0f;

    /// <summary>
    /// Personality's novelty sensitivity multiplier usually in the range: (0.5, 2.0)
    /// </summary>
    public float NoveltySensitivity { get; init; } = 1.0f;

    public EmotionDelta ApplyPersonalityTo(in EmotionDelta delta)
    {
        float threatStressScale = ThreatSensitivity * MathHelper.Lerp(1.25f, 0.55f, Bravery);
        float socialValenceScale = SocialSensitivity * MathHelper.Lerp(0.75f, 1.35f, Sociability);
        float noveltyScale = NoveltySensitivity * MathHelper.Lerp(0.75f, 1.35f, Curiosity);
        float neuroScale = MathHelper.Lerp(0.8f, 1.6f, Neuroticism);

        return new EmotionDelta
        {
            Valence = delta.Valence * (delta.Valence >= 0.0 ? socialValenceScale : neuroScale),
            Arousal = delta.Arousal * neuroScale,
            Dominance = delta.Dominance * MathHelper.Lerp(0.85f, 1.15f, Dominance),
            Stress = delta.Stress * threatStressScale * neuroScale,
            SocialNeed = delta.SocialNeed * MathHelper.Lerp(0.9f, 1.1f, Sociability),
            Curiosity = delta.Curiosity * noveltyScale,
            Fatigue = delta.Fatigue,
        };
    }
}