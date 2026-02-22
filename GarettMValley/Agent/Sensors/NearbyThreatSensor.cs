using StardewValley.Monsters;
using Microsoft.Xna.Framework;
using GarettMValley.Agent.Core;
using GarettMValley.Agent.Mind.Emotion;

namespace GarettMValley.Agent.Sensors;

public sealed class NearbyThreatSensor : ISensor
{
    private const float ThreatRadiusTiles = 8.0f;
    private const float ThreatRadiusTilesSq = ThreatRadiusTiles * ThreatRadiusTiles;
    private const int ThreatEmotionCooldown = 4; // ~1 second (AI ticks 4/sec)

    public void Sense(MindState mindstate)
    {
        Monster? nearest = null;
        float bestDistSq = float.MaxValue;

        foreach(var character in mindstate.Location!.characters)
        {
            if (character is not Monster monster)
                continue;
            float distanceSq = Vector2.DistanceSquared(monster.Tile, mindstate.Self!.Tile);
            if (distanceSq <= ThreatRadiusTilesSq && distanceSq < bestDistSq)
            {
                bestDistSq = distanceSq;
                nearest = monster;
            }
        }

        if (nearest is null)
            return;
        
        mindstate.ThreatNearby = true;
        mindstate.ThreatTile = nearest.Tile;

        if (mindstate.TryPulseCooldown("emotion:threat", ThreatEmotionCooldown))
        {
            EmotionDelta delta = new EmotionDelta
            {
                Valence = -0.06f,
                Arousal = +0.10f,
                Stress = +0.12f,
                Dominance = -0.04f,
                SocialNeed = -0.02f,
                Curiosity = -0.03f,
            };
            // clamp our value of `n` to 0 for far, and 1 for close.
            var best = MathF.Sqrt(bestDistSq);
            var n = 1.0f - MathHelper.Clamp(best / ThreatRadiusTiles, 0.0f, 1.0f);
            delta.Stress *= (0.5f + 0.5f * n);
            delta.Arousal *= (0.5f + 0.5f * n);

            mindstate.ApplyDelta(mindstate.Personality.ApplyPersonalityTo(delta));
        }
    }
}