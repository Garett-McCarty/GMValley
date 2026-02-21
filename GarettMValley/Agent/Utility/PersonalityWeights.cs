
using Microsoft.Xna.Framework;
using StardewValley;

namespace GarettMValley.Agent.Utility;

public static class PersonalityWeights
{
    public static float Social(in Blackboard blackboard)
    {
        float distance = blackboard.DistanceToPlayerTiles;
        float distanceW = 0.0f;
        if (!float.IsNegativeInfinity(distance) && !float.IsNaN(distance))
            distanceW = 1.0f - MathHelper.Clamp((distance - 3.0f) / 5.0f, 0.0f, 1.0f);
        float awareW = MathHelper.Clamp(blackboard.PlayerAwareness, 0.0f, 1.0f);
        return MathHelper.Clamp((0.55f * distanceW) + (0.45f * awareW), 0.0f, 1.0f);
    }

    public static float Threat(in Blackboard blackboard)
    {
        return blackboard.ThreatNearby ? 1.0f : 0.0f;
    }

    public static float Crowd(in Blackboard blackboard)
    {
        return MathHelper.Clamp(blackboard.NearbyAgentsCount / 6.0f, 0.0f, 1.0f);
    }

    public static float RelationshipHeartsToPlayer(in Blackboard blackboard)
    {
        if (blackboard.SelfRaw is not NPC npc || blackboard.Player is null)
            return 0.5f;
        if (blackboard.Player.friendshipData.TryGetValue(npc.Name, out var friendship))
        {
            float hearts = friendship.Points / 250.0f;
            return MathHelper.Clamp(hearts / 10.0f, 0.0f, 1.0f);
        }
        return 0.5f;
    }

    public static float CalmGate(in Blackboard blackboard)
    {
        return 1.0f - MathHelper.Clamp(blackboard.Emotion.Stress, 0.0f, 1.0f);
    }

    public static float PositiveMood(in Blackboard blackboard)
    {
        return MathHelper.Clamp((blackboard.Emotion.Valence + 1.0f) * 0.5f, 0.0f, 1.0f);
    }

    public static float NegativeMood(in Blackboard blackboard)
    {
        return MathHelper.Clamp((-blackboard.Emotion.Valence + 1.0f) * 0.5f, 0.0f, 1.0f);
    }
}