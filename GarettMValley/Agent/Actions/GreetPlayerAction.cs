using GarettMValley.Agent.Core;
using GarettMValley.Agent.Emote;
using GarettMValley.Agent.Utility;
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Actions;

public sealed class GreetPlayerAction : IAction
{
    public string IntentKey => "greet_player";
    public bool IsFinished { get; private set; } = false;
    public float Score(MindState mindstate)
    {
        if (!mindstate.PlayerIsNear) return 0.0f;
        if (mindstate.IsOnCooldown(IntentKey)) return 0.0f;
        if (mindstate.Self!.Kind != "villager") return 0.0f;

        float social = PersonalityWeights.Social(mindstate);
        float relationship = PersonalityWeights.RelationshipHeartsToPlayer(mindstate);
        float calm = PersonalityWeights.CalmGate(mindstate);
        float mood = PersonalityWeights.CalmGate(mindstate);
        float baseDesire = (0.40f * social) + (0.30f * relationship) + (0.20f * mood) + (0.10f * mindstate.Emotion.SocialNeed);
        float threat = 1.0f - PersonalityWeights.Threat(mindstate);
        float score = baseDesire * calm * threat;
        float seeFactor = MathHelper.Clamp(mindstate.PlayerAwareness, 0.0f, 1.0f);
        score *= (0.25f + 0.75f * seeFactor);
        return MathHelper.Clamp(score, 0.0f, 1.0f);
    }

    public bool CanContinue(MindState mindstate) => mindstate.PlayerIsNear;
    public void Start(MindState mindstate) { IsFinished = false; mindstate.StartIntent(IntentKey, ticks: 40); }
    public void Tick(MindState mindstate)
    {
        int emote = EmotePicker.Pick(mindstate.Emotion, mindstate.ThreatNearby, EmoteContext.Greeting);
        mindstate.Self!.FaceTile(mindstate.Player!.Tile);
        mindstate.Self.Emote(emote);
        mindstate.Self.Say("Hey [player]!");
        mindstate.SetCooldown(IntentKey, 600);
        IsFinished = true;
    }
    public void Abort(MindState mindstate) { IsFinished = true; mindstate.ClearIntent(); }
}