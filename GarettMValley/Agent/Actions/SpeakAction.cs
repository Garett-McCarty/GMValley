using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using GarettMValley;
using GarettMValley.Agent;
using GarettMValley.Agent.Utility;
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Actions;

public sealed class SpeakAction : IAction
{
    public string IntentKey => "speak";
    public bool IsFinished { get; private set; } = false;
    public float Score(Blackboard blackboard)
    {
        if (!blackboard.PlayerIsNear) return 0.0f;
        if (blackboard.IsOnCooldown("speak")) return 0.0f;
        
        float social = PersonalityWeights.Social(blackboard);
        float relationship = PersonalityWeights.RelationshipHeartsToPlayer(blackboard);
        float calm = PersonalityWeights.CalmGate(blackboard);
        float mood = PersonalityWeights.PositiveMood(blackboard);
        float threat = 1.0f - PersonalityWeights.Threat(blackboard);
        float baseDesire =
        (0.35f * social) +
        (0.25f * relationship) +
        (0.20f * mood) +
        (0.10f * blackboard.Emotion.SocialNeed) +
        (0.10f * MathHelper.Clamp(blackboard.Emotion.Curiosity, 0.0f, 1.0f));

        float crowdFactor = 1.0f - (0.35f * PersonalityWeights.Crowd(blackboard));
        float score = baseDesire * calm * threat * crowdFactor;

        score = 0.05f + (0.95f * score);

        float seeFactor = MathHelper.Clamp(blackboard.PlayerAwareness, 0.0f, 1.0f);
        score *= (0.25f + 0.75f * seeFactor);

        return MathHelper.Clamp(score, 0.0f, 1.0f);
    }
    public bool CanContinue(Blackboard _blackboard) => true;
    public void Start(Blackboard _blackboard) { IsFinished = false; }
    public void Tick(Blackboard blackboard)
    { 
        blackboard.Self!.Say("This is speech from SpeakAction!");
        blackboard.SetCooldown(IntentKey, 600);
        IsFinished = true;
    }
    public void Abort(Blackboard _blackboard) { IsFinished = true; }
}