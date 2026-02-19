
using GarettMValley.AI.Emote;
using GarettMValley.AI.Utility;
using Microsoft.Xna.Framework;

namespace GarettMValley.AI.Actions;

public sealed class GreetPlayerAction : IAction
{
    public string IntentKey => "greet_player";
    public bool IsFinished { get; private set; } = false;
    public float Score(Blackboard blackboard)
    {
        if (!blackboard.PlayerIsNear) return 0.0f;
        if (blackboard.IsOnCooldown("greet_player")) return 0.0f;
        if (blackboard.Self!.Kind != "villager") return 0.0f;

        float social = PersonalityWeights.Social(blackboard);
        float relationship = PersonalityWeights.RelationshipHeartsToPlayer(blackboard);
        float calm = PersonalityWeights.CalmGate(blackboard);
        float mood = PersonalityWeights.CalmGate(blackboard);
        float baseDesire = (0.40f * social) + (0.30f * relationship) + (0.20f * mood) + (0.10f * blackboard.Emotion.SocialNeed);
        float threat = 1.0f - PersonalityWeights.Threat(blackboard);
        float score = baseDesire * calm * threat;
        return MathHelper.Clamp(score, 0.0f, 1.0f);
    }

    public bool CanContinue(Blackboard blackboard) => blackboard.PlayerIsNear;
    public void Start(Blackboard blackboard) { IsFinished = false; blackboard.StartIntent(IntentKey, ticks: 40); }
    public void Tick(Blackboard blackboard)
    {
        int emote = EmotePicker.Pick(blackboard.Emotion, blackboard.ThreatNearby, EmoteContext.Greeting);
        blackboard.Self!.FaceTile(blackboard.Player!.Tile);
        blackboard.Self.Emote(20);
        blackboard.Self.Say("Hey [player]!");
        blackboard.SetCooldown("greet_player", 600);
        IsFinished = true;
    }
    public void Abort(Blackboard blackboard) { IsFinished = true; blackboard.ClearIntent(); }
}