
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using GarettMValley.AI;

namespace GarettMValley;

/// <summary>
/// GarettM Valley - AI Agent MOD!
/// </summary>
public sealed class ModEntry : Mod
{
    /// <summary>
    /// Instance to our mod configuration
    /// </summary>
    private ModConfig Config = new();

    /// <summary>
    /// Instance to our AI Manager
    /// </summary>
    private AiManager AiManager = null!;

    private DialogueSystem DialogueManager = null!;


    /// <summary>
    /// Mod entry point
    /// </summary>
    /// <param name="helper"></param>
    public override void Entry(IModHelper helper)
    {
        try
        {
            Config = helper.ReadConfig<ModConfig>();
            Monitor.Log("GarettMValley loading…", LogLevel.Info);
            
            AiManager = new AiManager(Monitor, Config);
            AiManager.Hook(helper);
            
            DialogueManager = new DialogueSystem(Monitor);
            DialogueManager.Hook(helper, AiManager, this.ModManifest.UniqueID);
            
            Monitor.Log("GarettMValley loaded successfully with AI dialogue support!", LogLevel.Info);
        }
        catch (Exception ex)
        {
            Monitor.Log($"CRITICAL ERROR during mod initialization!", LogLevel.Error);
            Monitor.Log($"Exception Type: {ex.GetType().FullName}", LogLevel.Error);
            Monitor.Log($"Exception Message: {ex.Message}", LogLevel.Error);
            Monitor.Log($"Stack Trace: {ex.StackTrace}", LogLevel.Error);
            if (ex.InnerException != null)
            {
                Monitor.Log($"Inner Exception: {ex.InnerException.Message}", LogLevel.Error);
                Monitor.Log($"Inner Stack Trace: {ex.InnerException.StackTrace}", LogLevel.Error);
            }
            throw;
        }
    }
}
