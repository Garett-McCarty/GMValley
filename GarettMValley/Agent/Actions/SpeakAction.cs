
using GarettMValley.Agent.Core;
using GarettMValley.Agent.Utility;
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Actions;

public sealed class SpeakAction : IAction
{
    public string IntentKey => "speak";
    public bool IsFinished { get; private set; } = false;
    public float Score(MindState mindstate)
    {
        if (!mindstate.PlayerIsNear) return 0.0f;
        if (mindstate.IsOnCooldown("speak")) return 0.0f;
        
        float social = PersonalityWeights.Social(mindstate);
        float relationship = PersonalityWeights.RelationshipHeartsToPlayer(mindstate);
        float calm = PersonalityWeights.CalmGate(mindstate);
        float mood = PersonalityWeights.PositiveMood(mindstate);
        float threat = 1.0f - PersonalityWeights.Threat(mindstate);
        float baseDesire =
        (0.35f * social) +
        (0.25f * relationship) +
        (0.20f * mood) +
        (0.10f * mindstate.Emotion.SocialNeed) +
        (0.10f * MathHelper.Clamp(mindstate.Emotion.Curiosity, 0.0f, 1.0f));

        float crowdFactor = 1.0f - (0.35f * PersonalityWeights.Crowd(mindstate));
        float score = baseDesire * calm * threat * crowdFactor;

        score = 0.05f + (0.95f * score);

        float seeFactor = MathHelper.Clamp(mindstate.PlayerAwareness, 0.0f, 1.0f);
        score *= (0.25f + 0.75f * seeFactor);

        return MathHelper.Clamp(score, 0.0f, 1.0f);
    }
    public bool CanContinue(MindState _mindstate) => true;
    public void Start(MindState _mindstate) { IsFinished = false; }
    public void Tick(MindState mindstate)
    { 
        mindstate.Self!.Say("This is speech from SpeakAction!");
        mindstate.SetCooldown(IntentKey, 600);
        IsFinished = true;
    }
    public void Abort(MindState _mindstate) { IsFinished = true; }
}