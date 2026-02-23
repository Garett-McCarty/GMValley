
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
using GarettMValley.Cheats;
using GarettMValley.Cheats.Cheats;

namespace GarettMValley;

/// <summary>
/// GarettM Valley - AI Enhancement and Quality Mod
/// </summary>
public sealed class ModEntry : Mod
{
    /// <summary>
    /// Mod Configuration
    /// </summary>
    internal ModConfig _config { get; private set; } = new();

    /// <summary>
    /// Agent Manager
    /// </summary>
    private AgentManager _agentManager = null!;

    /// <summary>
    /// Cheat Manager
    /// </summary>
    private CheatManager _cheats = null!;

    /// <summary>
    /// Debug Web Server
    /// </summary>
    private DebugServer _apiServer = null!;

    /// <summary>
    /// Dialogue system
    /// </summary>
    private DialogueSystem _dialogueManager = null!;

    /// <summary>
    /// UI system
    /// </summary>
    private UiManager _uiManager = null!;

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
                Monitor.Log("GarettMValley is disabled in mod's configuration, Bailing out!");
                return;
            }

            Monitor.Log("GarettMValley loading…", LogLevel.Info);
            
            _agentManager = new AgentManager(Monitor, _config);
            _agentManager.Hook(helper);

            _cheats = new CheatManager(Monitor);
            _cheats.Register(new ToggleAgentDebugCheat());
            _cheats.Register(new ClearAgentsCheat());
            _cheats.Register(new RegeneratePersonalitiesCheat());
            _cheats.Register(new ToggleDialogueCheat());
            _cheats.Register(new SetNearestNpcEmotionCheat());

            helper.ConsoleCommands.Add(
                name: "gmv_cheat",
                documentation: "Run a GMValley cheat. Usage: gmv_cheat <id> [args...]",
                callback: (cmd, args) =>
                {
                    if (args.Length <= 0)
                    {
                        Monitor.Log("Missing cheat id. Example: gmv_cheat agent_debug", LogLevel.Info);
                        Monitor.Log("Available cheats: " + string.Join(", ", _cheats.All.Select(c => c.Id)), LogLevel.Info);
                        return;
                    }

                    var id = args[0];
                    var rest = args.Skip(1).ToArray();
                    var context = new CheatContext(Monitor, helper, _config, _agentManager, _dialogueManager);

                    if (!_cheats.TryRun(id, context, rest, out var error) && !string.IsNullOrWhiteSpace(error))
                        Monitor.Log(error, LogLevel.Warn);
                }
            );
            
            _dialogueManager = new DialogueSystem(Monitor);
            _dialogueManager.Hook(helper, _agentManager, this.ModManifest.UniqueID);

            _apiServer = new DebugServer(Monitor, _agentManager, _config);
            _apiServer.Start();

            _uiManager = new UiManager(Monitor, Helper, () => this._config, (cfg) => this._config = cfg, (cfg) =>
                {
                    // TODO: Propagate to different subsystems that our configuration has been updated.
                },
                cheats: _cheats,
                getCheatContext: () => new CheatContext(Monitor, helper, _config, _agentManager, _dialogueManager)
            );
            _uiManager.Initialize();

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
