using HarmonyLib;
using StardewValley;

namespace GarettMValley.AI;

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
        // If our system isn't available, do nothing.
        var system = DialogueSystemBridge.System;
        if (system is null)
            return true;

        // Ask the system if it wants to handle this action.
        // Note: We call the async method but block on it since Harmony prefixes must be synchronous
        try
        {
            var task = system.TryHandleNpcAction(__instance, who, l);
            if (task.IsCompleted)
            {
                if (task.Result)
                {
                    __result = true;
                    return false;
                }
            }
            else
            {
                // If not completed, queue it to run later without blocking
                task.ContinueWith(t =>
                {
                    if (t.Result)
                    {
                        // We can't modify __result here, so dialogue was already shown in the async method
                    }
                });
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
