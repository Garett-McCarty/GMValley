
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Characters;
using GarettMValley.Agent.Core;
using GarettMValley.Agent.Mind.Emotion;

namespace GarettMValley.Agent.Sensors;

public sealed class NearbyAgentSensor : ISensor
{
    private const float RadiusTiles = 6.0f;
    private const float RadiusTilesSq = RadiusTiles * RadiusTiles;
    private const int SocialPulseCooldown = 4; // ~1s
    private const int CrowdPulseCooldown = 12; // ~3s

    public void Sense(MindState mindstate)
    {
        int count = 0;
        string? nearestId = null;
        Vector2 nearestTile = default;
        float nearestDistSq = float.MaxValue;

        foreach (var character in mindstate.Location!.characters)
        {
            if (character is null) continue;
            if (ReferenceEquals(character, mindstate.SelfRaw)) continue;

            if (character is not NPC npc) continue;
            if (character is Pet) continue;

            float distanceSq = Vector2.DistanceSquared(npc.Tile, mindstate.Self!.Tile);
            if (distanceSq <= RadiusTilesSq)
            {
                count += 1;
                if (distanceSq < nearestDistSq)
                {
                    nearestDistSq = distanceSq;
                    nearestId = npc.Name;
                    nearestTile = npc.Tile;
                }
            }

        }

        mindstate.NearbyAgentsCount = count;
        mindstate.NearestFriendlyId = nearestId;
        mindstate.NearestFriendlyTile = nearestTile;
        mindstate.DistanceToNearestFriendly = nearestDistSq == float.MaxValue ? float.MaxValue : MathF.Sqrt(nearestDistSq);

        if (count <= 0)
            return;
        
        if (mindstate.TryPulseCooldown("emotion:social_nearby", SocialPulseCooldown))
        {
            float n = MathHelper.Clamp(count / 4.0f, 0.0f, 1.0f);
            EmotionDelta delta = new EmotionDelta
            {
                Valence = +0.03f * n,
                SocialNeed = -0.08f * n,
                Arousal = +0.02f * n,
            };

            mindstate.ApplyDelta(mindstate.Personality.ApplyPersonalityTo(delta));
        }

        if (count >= 5 && mindstate.TryPulseCooldown("emotion:social_crowd", CrowdPulseCooldown))
        {
            float n = MathHelper.Clamp((count - 4) / 6.0f, 0.0f, 1.0f);
            EmotionDelta delta = new EmotionDelta
            {
                Stress = +0.04f * n,
                Dominance = -0.02f * n,
            };

            mindstate.ApplyDelta(mindstate.Personality.ApplyPersonalityTo(delta));
        }
    }
}