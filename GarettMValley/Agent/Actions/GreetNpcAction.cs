using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using GarettMValley;
using GarettMValley.Agent;
using System.Security.Cryptography.X509Certificates;
using GarettMValley.Agent.Emote;
using GarettMValley.Agent.Utility;
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Actions;

public sealed class GreetNpcAction : IAction
{
    public string IntentKey => "greet_npc";
    public bool IsFinished { get; private set; } = false;
    public float Score(Blackboard blackboard)
    {
        var distance = blackboard.DistanceToNearestFriendly;
        if (float.IsNegativeInfinity(distance) || float.IsNaN(distance))
            return 0.0f;

        if (blackboard.IsOnCooldown(IntentKey)) return 0.0f;
        if (blackboard.NearestFriendlyId is null) return 0.0f;
        if (distance > 3.0) return 0.0f;

        float threat = 1.0f - PersonalityWeights.Threat(blackboard);
        if (threat <= 0.0f) return 0.0f;

        float nearNpc = 1.0f - MathHelper.Clamp((distance - 2.0f) / (7.0f - 2.0f), 0.0f, 1.0f);
        float calm = PersonalityWeights.CalmGate(blackboard);
        float mood = PersonalityWeights.PositiveMood(blackboard);
        float baseDesire =
            (0.55f * nearNpc) +
            (0.25f * blackboard.Emotion.SocialNeed) +
            (0.20f * mood);
        float crowd = 1.0f - (0.25f * PersonalityWeights.Crowd(blackboard));
        float score = baseDesire * calm * threat * crowd;

        return MathHelper.Clamp(score, 0.0f, 1.0f);
    }
    public bool CanContinue(Blackboard blackboard) => blackboard.NearestFriendlyId is not null && blackboard.DistanceToNearestFriendly <= 4.0f;
    public void Start(Blackboard blackboard) { IsFinished = false; blackboard.StartIntent(IntentKey, 30); }
    public void Tick(Blackboard blackboard)
    {
        int emote = EmotePicker.Pick(blackboard.Emotion, blackboard.ThreatNearby, EmoteContext.Greeting);
        blackboard.Self!.FaceTile(blackboard.NearestFriendlyTile);
        blackboard.Self.Emote(emote);
        blackboard.SetCooldown(IntentKey, 900);
        IsFinished = true;
    }
    public void Abort(Blackboard blackboard) { IsFinished = true; blackboard.ClearIntent(); }
}