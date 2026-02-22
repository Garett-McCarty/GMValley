using System.Globalization;
using StardewModdingAPI;

namespace GarettMValley.Cheats.Cheats;

internal sealed class SetNearestNpcEmotionCheat : ICheat
{
    public string Id => "set_emotion";
    public string Name => "Set nearest NPC emotion";
    public string Description => "Usage: set_emotion <valence -1..1> <arousal 0..1> <dominance 0..1> <stress 0..1> [radiusTiles].";
    public bool IsToggle => false;

    public bool CanRun(CheatContext context, out string? reason)
    {
        reason = null;
        return true;
    }

    public void Run(CheatContext context, string[] args)
    {
        if (args.Length < 4)
        {
            context.monitor.Log("set_emotion requires 4 numbers: valence arousal dominance stress (optional: radiusTiles)", StardewModdingAPI.LogLevel.Info);
            return;
        }

        static bool TryParse(string s, out float v)
            => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);

        if (!TryParse(args[0], out var valence) ||
            !TryParse(args[1], out var arousal) ||
            !TryParse(args[2], out var dominance) ||
            !TryParse(args[3], out var stress))
        {
            context.monitor.Log("set_emotion: failed to parse numbers. Tip: use '.' for decimals.", StardewModdingAPI.LogLevel.Info);
            return;
        }

        float radius = 6f;
        if (args.Length >= 5 && TryParse(args[4], out var r))
            radius = r;

        if (context.agents.TrySetNearestNpcEmotion(valence, arousal, dominance, stress, radius, out var npcName))
        {
            context.monitor.Log($"Set emotion for nearest NPC '{npcName}' to V={valence:0.00} A={arousal:0.00} D={dominance:0.00} S={stress:0.00}", StardewModdingAPI.LogLevel.Info);
        }
        else
        {
            context.monitor.Log("No NPC found nearby to set emotion.", LogLevel.Info);
        }
    }
}
