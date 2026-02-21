
using Microsoft.Xna.Framework;
using StardewValley;

namespace GarettMValley.Agent.Mind;

public static class VisualAppraisal
{
    public static (float appraisal, float threat, float novelty) Appraise(MindState mindstate, object target, string targetKind, string targetId)
    {
        float appraisal = 0.0f;
        float threat = 0.0f;
        float novelty = 0.0f;

        if (targetKind == "player" && mindstate.Player is not null && mindstate.SelfRaw is NPC npc)
        {
            float heartsFactor = 0.5f;
            if (mindstate.Player.friendshipData.TryGetValue(npc.Name, out var friendship))
            {
                float hearts = friendship.Points / 250.0f;
                heartsFactor = MathHelper.Clamp(hearts / 10.0f, 0.0f, 1.0f);
            }

            appraisal = MathHelper.Clamp(-0.2f + 1.2f * heartsFactor, -1.0f, 1.0f);

            if (mindstate.VisuallyPerceived.TryGetValue("player", out var player))
            {
                novelty = MathHelper.Clamp((player.LastSeenTick == 0 ? 1.0f : (mindstate.Tick - player.LastSeenTick) / 40.0f), 0.0f, 1.0f);
            }
        }

        if (targetKind == "monster")
        {
            appraisal -= 0.8f;
            threat = 1.0f;
            novelty = 0.2f;
        }

        return (appraisal, threat, novelty);
    }
}