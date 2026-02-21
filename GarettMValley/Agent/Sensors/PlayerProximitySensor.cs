
using GarettMValley.Agent.Mind.Emotion;
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Sensors;

public sealed class PlayerProximitySensor : ISensor
{
    private const float NearRadiusTiles = 4.0f;
    private const float NearRadiusTilesSq = NearRadiusTiles * NearRadiusTiles;
    private const int NearPulseCooldown = 4;
    private const int FarPulseCooldown = 20;

    public void Sense(Blackboard blackboard)
    {
        float distanceSq = Vector2.DistanceSquared(blackboard.Self!.Tile, blackboard.Player!.Tile);
        blackboard.DistanceToPlayerTiles = MathF.Sqrt(distanceSq);
        bool isNear = distanceSq < NearRadiusTilesSq;
        blackboard.PlayerIsNear = isNear;
        if (isNear)
        {
            if(blackboard.TryPulseCooldown("emotion:player_near", NearPulseCooldown))
            {
                float n = 1.0f - MathHelper.Clamp(blackboard.DistanceToPlayerTiles / NearRadiusTiles, 0.0f, 1.0f);

                EmotionDelta delta = new EmotionDelta
                {
                    Valence = +0.04f * n,
                    Stress = -0.06f * n,
                    SocialNeed = -0.08f * n,
                    Arousal = +0.02f * n,
                    Dominance = +0.01f * n,
                };

                blackboard.ApplyDelta(blackboard.Personality.ApplyPersonalityTo(delta));
            };
        }
        else
        {
            if (blackboard.TryPulseCooldown("emotion:player_far", FarPulseCooldown))
            {
                EmotionDelta delta = new EmotionDelta
                {
                    Valence = -0.02f,
                    Stress = +0.02f,
                    SocialNeed = +0.06f,
                    Curiosity = +0.01f,
                };
                blackboard.ApplyDelta(blackboard.Personality.ApplyPersonalityTo(delta));
            }
        }
    }
}