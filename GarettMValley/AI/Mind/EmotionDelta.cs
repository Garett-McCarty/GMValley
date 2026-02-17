
namespace GarettMValley.AI.Mind;

/// <summary>
/// Emotion delta
/// </summary>
public struct EmotionDelta
{
    public float Valence;
    public float Arousal;
    public float Dominance;
    public float Stress;
    public float SocialNeed;
    public float Curiosity;
    public float Fatigue;

    public static EmotionDelta Zero => default;

    public EmotionDelta Scale(float scale_factor) => new()
    {
        Valence = Valence * scale_factor,
        Arousal = Arousal * scale_factor,
        Dominance = Dominance * scale_factor,
        Stress = Stress * scale_factor,
        SocialNeed = SocialNeed * scale_factor,
        Curiosity = Curiosity * scale_factor,
        Fatigue = Fatigue * scale_factor,
    };
}