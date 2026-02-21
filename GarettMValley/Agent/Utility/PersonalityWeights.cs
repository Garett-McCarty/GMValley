using GarettMValley.Agent.Mind;
using Microsoft.Xna.Framework;
using StardewValley;

namespace GarettMValley.Agent.Utility;

public static class PersonalityWeights
{
    public static float Social(in MindState mindstate)
    {
        float distance = mindstate.DistanceToPlayerTiles;
        float distanceW = 0.0f;
        if (!float.IsNegativeInfinity(distance) && !float.IsNaN(distance))
            distanceW = 1.0f - MathHelper.Clamp((distance - 3.0f) / 5.0f, 0.0f, 1.0f);
        float awareW = MathHelper.Clamp(mindstate.PlayerAwareness, 0.0f, 1.0f);
        return MathHelper.Clamp((0.55f * distanceW) + (0.45f * awareW), 0.0f, 1.0f);
    }

    public static float Threat(in MindState mindstate)
    {
        return mindstate.ThreatNearby ? 1.0f : 0.0f;
    }

    public static float Crowd(in MindState mindstate)
    {
        return MathHelper.Clamp(mindstate.NearbyAgentsCount / 6.0f, 0.0f, 1.0f);
    }

    public static float RelationshipHeartsToPlayer(in MindState mindstate)
    {
        if (mindstate.SelfRaw is not NPC npc || mindstate.Player is null)
            return 0.5f;
        if (mindstate.Player.friendshipData.TryGetValue(npc.Name, out var friendship))
        {
            float hearts = friendship.Points / 250.0f;
            return MathHelper.Clamp(hearts / 10.0f, 0.0f, 1.0f);
        }
        return 0.5f;
    }

    public static float CalmGate(in MindState mindstate)
    {
        return 1.0f - MathHelper.Clamp(mindstate.Emotion.Stress, 0.0f, 1.0f);
    }

    public static float PositiveMood(in MindState mindstate)
    {
        return MathHelper.Clamp((mindstate.Emotion.Valence + 1.0f) * 0.5f, 0.0f, 1.0f);
    }

    public static float NegativeMood(in MindState mindstate)
    {
        return MathHelper.Clamp((-mindstate.Emotion.Valence + 1.0f) * 0.5f, 0.0f, 1.0f);
    }
}