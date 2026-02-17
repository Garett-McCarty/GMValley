using Microsoft.Xna.Framework;

namespace GarettMValley.AI.Mind;

public struct EmotionalState
{
    public float Valence;
    public float Arousal;
    public float Dominance;

    public float Stress;
    public float SocialNeed;
    public float Curiosity;
    public float Fatigue;

    public static EmotionalState Neutral => new()
    {
        Valence = 0.0f,
        Arousal = 0.25f,
        Dominance = 0.5f,
        Stress = 0.15f,
        SocialNeed = 0.35f,
        Curiosity = 0.35f,
        Fatigue = 0.15f
    };

    /// <summary>
    /// Normalize all channels to safe ranges.
    /// </summary>
    public void Normalize()
    {
        Valence = MathHelper.Clamp(Valence, -1.0f, 1.0f);
        Arousal = MathHelper.Clamp(Arousal, 0.0f, 1.0f);
        Dominance = MathHelper.Clamp(Dominance, 0.0f, 1.0f);
        Stress = MathHelper.Clamp(Stress, 0.0f, 1.0f);
        SocialNeed = MathHelper.Clamp(SocialNeed, 0.0f, 1.0f);
        Curiosity = MathHelper.Clamp(Curiosity, 0.0f, 1.0f);
        Fatigue = MathHelper.Clamp(Fatigue, 0.0f, 1.0f);
    }

    public void DecayToward(in EmotionalState baseline, float delta, float halflife)
    {
        float n = 1.0f - MathF.Pow(0.5f, delta / MathF.Max(0.001f, halflife));
        Valence = MathHelper.Lerp(Valence, baseline.Valence, n);
        Arousal = MathHelper.Lerp(Arousal, baseline.Arousal, n);
        Dominance = MathHelper.Lerp(Dominance, baseline.Dominance, n);
        Stress = MathHelper.Lerp(Stress, baseline.Stress, n);
        SocialNeed = MathHelper.Lerp(SocialNeed, baseline.SocialNeed, n);
        Curiosity = MathHelper.Lerp(Curiosity, baseline.Curiosity, n);
        Fatigue = MathHelper.Lerp(Fatigue, baseline.Fatigue, n);
        Normalize();
    }

    public void Apply(in EmotionDelta delta)
    {
        Valence += delta.Valence;
        Arousal += delta.Arousal;
        Dominance += delta.Dominance;
        Stress += delta.Stress;
        SocialNeed += delta.SocialNeed;
        Curiosity += delta.Curiosity;
        Fatigue += delta.Fatigue;
        Normalize();
    }
}