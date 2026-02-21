
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using GarettMValley.Agent;
using GarettMValley.Dialogue;
using GarettMValley.Network;
using GarettMValley.UI;

namespace GarettMValley;

/// <summary>
/// GarettM Valley - AI Agent MOD!
/// </summary>
public sealed class ModEntry : Mod
{
    /// <summary>
    /// MOD Configuration
    /// </summary>
    internal ModConfig _config { get; private set; } = new();

    /// <summary>
    /// Agent Manager
    /// </summary>
    private AgentManager _agentManager = null!;

    /// <summary>
    /// Debug Web Server
    /// </summary>
    private DebugServer _apiServer = null!;

    private DialogueSystem DialogueManager = null!;

    private UiManager UiManager = null!;


    /// <summary>
    /// Mod entry point
    /// </summary>
    /// <param name="helper"></param>
    public override void Entry(IModHelper helper)
    {
        try
        {
            _config = helper.ReadConfig<ModConfig>();
            if (!_config.EnableMod)
            {
                Monitor.Log("GarettMValley is disabled in MOD Configuration, Bailing out!");
                return;
            }

            Monitor.Log("GarettMValley loading…", LogLevel.Info);
            
            _agentManager = new AgentManager(Monitor, _config);
            _agentManager.Hook(helper);
            
            DialogueManager = new DialogueSystem(Monitor);
            DialogueManager.Hook(helper, _agentManager, this.ModManifest.UniqueID);

            _apiServer = new DebugServer(Monitor, _agentManager, _config);
            _apiServer.Start();

            UiManager = new UiManager(Monitor, Helper, () => this._config, (cfg) => this._config = cfg, (cfg) =>
            {
                // TODO: Propagate to different subsystems that our configuration has been updated.
            });
            UiManager.Initialize();

            helper.Events.GameLoop.ReturnedToTitle += (_, _) => _apiServer?.Stop();
            helper.Events.GameLoop.GameLaunched += (_, _) => _apiServer?.Start();
            helper.Events.GameLoop.SaveLoaded += (_, _) => _apiServer?.Start();
            
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
