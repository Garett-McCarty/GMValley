using GarettMValley.Agent.Core;
using GarettMValley.Agent.Emote;
using GarettMValley.Agent.Utility;
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Actions;

public sealed class GreetNpcAction : IAction
{
    public string IntentKey => "greet_npc";
    public bool IsFinished { get; private set; } = false;
    public float Score(MindState mindstate)
    {
        var distance = mindstate.DistanceToNearestFriendly;
        if (float.IsNegativeInfinity(distance) || float.IsNaN(distance))
            return 0.0f;

        if (mindstate.IsOnCooldown(IntentKey)) return 0.0f;
        if (mindstate.NearestFriendlyId is null) return 0.0f;
        if (distance > 3.0) return 0.0f;

        float threat = 1.0f - PersonalityWeights.Threat(mindstate);
        if (threat <= 0.0f) return 0.0f;

        float nearNpc = 1.0f - MathHelper.Clamp((distance - 2.0f) / (7.0f - 2.0f), 0.0f, 1.0f);
        float calm = PersonalityWeights.CalmGate(mindstate);
        float mood = PersonalityWeights.PositiveMood(mindstate);
        float baseDesire =
            (0.55f * nearNpc) +
            (0.25f * mindstate.Emotion.SocialNeed) +
            (0.20f * mood);
        float crowd = 1.0f - (0.25f * PersonalityWeights.Crowd(mindstate));
        float score = baseDesire * calm * threat * crowd;

        return MathHelper.Clamp(score, 0.0f, 1.0f);
    }
    public bool CanContinue(MindState mindstate) => mindstate.NearestFriendlyId is not null && mindstate.DistanceToNearestFriendly <= 4.0f;
    public void Start(MindState mindstate) { IsFinished = false; mindstate.StartIntent(IntentKey, 30); }
    public void Tick(MindState mindstate)
    {
        int emote = EmotePicker.Pick(mindstate.Emotion, mindstate.ThreatNearby, EmoteContext.Greeting);
        mindstate.Self!.FaceTile(mindstate.NearestFriendlyTile);
        mindstate.Self.Emote(emote);
        mindstate.SetCooldown(IntentKey, 900);
        IsFinished = true;
    }
    public void Abort(MindState mindstate) { IsFinished = true; mindstate.ClearIntent(); }
}