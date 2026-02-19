namespace GarettMValley.AI.Emote;

public static class EmotePicker
{
    public static int Pick(in Mind.Emotion.EmotionalState emotionalState, bool threatNearby, EmoteContext emoteContext)
    {
        if (threatNearby || emotionalState.Stress >= 0.75f)
        {
            return emotionalState.Dominance >= 0.55f ? EmoteIds.Angry : EmoteIds.Exclamation;
        }

        if (emotionalState.Fatigue >= 0.70f)
            return EmoteIds.Sleep;
        
        if (emoteContext is EmoteContext.Greeting or EmoteContext.Talking)
        {
            if (emotionalState.Valence >= 0.55f)
                return EmoteIds.Heart;
            if (emotionalState.Valence <= -0.45f)
                return (emotionalState.Dominance >= 0.55f) ? EmoteIds.Angry : EmoteIds.Sad;
            if (emotionalState.Curiosity >= 0.60f)
                return EmoteIds.Question;
            if (emotionalState.Valence >= 0.20f)
                return EmoteIds.Happy;
            return EmoteIds.Dots;
        }

        if (emotionalState.Curiosity >= 0.65f && emotionalState.Arousal <= 0.50f)
            return EmoteIds.Question;
        if (emotionalState.Valence >= 0.30f)
            return EmoteIds.Happy;
        if (emotionalState.Valence <= -0.40f)
            return EmoteIds.Sad;
        return EmoteIds.Dots;
    }
}