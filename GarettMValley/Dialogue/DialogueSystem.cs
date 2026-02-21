using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

using GarettMValley.Agent;
using GarettMValley.Scheduler;

namespace GarettMValley.Dialogue;

/// <summary>
/// Central dialogue takeover system. Installs Harmony patches and routes talk interactions to agent brains.
/// </summary>
public sealed class DialogueSystem : IDisposable
{
    /// <summary>
    /// Reference to our monitoring service provided by SMAPI
    /// </summary>
    private readonly IMonitor _monitor;

    /// <summary>
    /// Reference to our Dialogue Service to generate dialogue
    /// </summary>
    private DialogueService? _dialogueService;

    /// <summary>
    /// Reference to our AI Manager
    /// </summary>
    private AgentManager? _ai;

    /// <summary>
    /// Flag to determine if we enable the dialogue system
    /// </summary>
    public bool Enabled { get; set; } = true;

    public DialogueSystem(IMonitor monitor)
    {
        _monitor = monitor;
    }

    public void Hook(IModHelper helper, AgentManager aiManager, string harmonyId)
    {
        _ai = aiManager;
        if (_ai.GetConfig().AllowDialogue)
        {
            Enabled = true;
        } else
        {
            Enabled = false;
        }

        var dispatcher = new MainThreadDispatcher(helper, _monitor);
        _dialogueService = new DialogueService(monitor: _monitor, dispatcher: dispatcher, ollamaUri: _ai.GetConfig().OllamaUrl, ollamaModel: _ai.GetConfig().OllamaModel, _ai.GetConfig().OllamaTimeout, this);
        
        // Optional: quick toggle keys
        helper.Events.Input.ButtonPressed += OnButtonPressed;
        helper.Events.Display.MenuChanged += OnMenuChanged;

        // Make this instance globally reachable by patch code
        DialogueSystemBridge.System = this;

        // Install Harmony patch(es)
        var harmony = new HarmonyLib.Harmony(harmonyId);
        DialogueHarmonyPatches.Apply(harmony);
    }

    public void Dispose()
    {
        _dialogueService?.Dispose();
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        if (e.Button == SButton.F12)
        {
            Enabled = !Enabled;
            _monitor.Log($"DialogueSystem Enabled={Enabled}", LogLevel.Info);
        }
    }

    /// <summary>
    /// Cancel active AI generation when dialogue closes.
    /// This prevents ghost responses after player walks away.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.OldMenu is DialogueBox && e.NewMenu is not DialogueBox)
        {
            _dialogueService?.CancelActive();
        }
    }

    /// <summary>
    /// Entry point called from Harmony patch. Returns true if we handled the interaction (skip vanilla).
    /// </summary>
    internal bool TryHandleNpcAction(NPC npc, Farmer who, GameLocation location)
    {
        if (!Enabled || _ai is null || _dialogueService is null)
        {
            return false; // Disabled or DialogueService not available
        }

        // Hard safety guards: don't break scripted content.
        if (Game1.eventUp)
            return false;

        // Validate that farmer is present
        if (who is null)
            return false;

        // If player is holding an object, vanilla often uses that for gifts/etc.
        if (who.ActiveObject is not null)
            return false;

        // Ask AI brain: should we override right now? Decide if we *want* to takeover
        if (!_ai.TryDecideTakeover(npc, who, location, out var takeover) || !takeover.ShouldTakeOver)
            return false;

        ShowDialogue(npc, "thinking...");

        _dialogueService.StartNpcDialogue(
            npc,
            getSystemPrompt: () => _ai.GetSystemPromptForNPC(npc),
            getUserPrompt: (playerText) => _ai.GetUserPromptFor(npc, who, location, playerText),
            getNpcName: () => npc.Name
        );
        return true;
    }

    public static void ShowDialogue(NPC npc, string text)
    {
        var dialogue = new Stack<StardewValley.Dialogue>();
        dialogue.Push(new StardewValley.Dialogue(npc, "", text));
        npc.TemporaryDialogue = dialogue;
        Game1.drawDialogue(npc);
    }
}
