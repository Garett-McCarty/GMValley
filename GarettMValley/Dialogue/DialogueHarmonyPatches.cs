using HarmonyLib;
using StardewValley;

namespace GarettMValley.Dialogue;

/// <summary>
/// Harmony patches that intercept talk interactions.
/// </summary>
internal static class DialogueHarmonyPatches
{
    internal static void Apply(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
            prefix: new HarmonyMethod(typeof(DialogueHarmonyPatches), nameof(NPC_checkAction_Prefix))
        );
    }

    /// <summary>
    /// Prefix return:
    /// - true  => let vanilla run
    /// - false => skip vanilla (we handled it)
    /// </summary>
    private static bool NPC_checkAction_Prefix(NPC __instance, Farmer who, GameLocation l, ref bool __result)
    {
        // If our system isn't available, let vanilla handle it
        var system = DialogueSystemBridge.System;
        if (system is null)
            return true;

        try
        {
            if (system.TryHandleNpcAction(__instance, who, l))
            {
                __result = true;
                return false;
            }
        }
        catch
        {
            // If there's an error, let vanilla handle it
        }

        return true;
    }
}

/// <summary>
/// Simple bridge so Harmony patch can reach your DialogueSystem instance.
/// </summary>
internal static class DialogueSystemBridge
{
    public static DialogueSystem? System;
}
