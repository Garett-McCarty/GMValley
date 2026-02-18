using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace GarettMValley.AI;

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
    private AiManager? _ai;

    /// <summary>
    /// Flag to determine if we enable the dialogue system
    /// </summary>
    public bool Enabled { get; set; } = true;

    public DialogueSystem(IMonitor monitor)
    {
        _monitor = monitor;
    }

    public void Hook(IModHelper helper, AiManager aiManager, string harmonyId)
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
        _dialogueService = new DialogueService(monitor: _monitor, dispatcher: dispatcher, ollamaUri: _ai.GetConfig().OllamaUrl, ollamaModel: _ai.GetConfig().OllamaModel, _ai.GetConfig().OllamaTimeout);
        
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
    internal async Task<bool> TryHandleNpcAction(NPC npc, Farmer who, GameLocation location)
    {
        if (!Enabled || _ai is null || _dialogueService is null)
        {
            return false; // Disabled or DialogueService not available
        }

        // Hard safety guards: don't break scripted content.
        if (Game1.eventUp || Game1.activeClickableMenu is not null)
            return false;

        // Validate that farmer is present
        if (who is null)
            return false;

        // If player is holding an object, vanilla often uses that for gifts/etc.
        // You can refine later (e.g. allow tools but not objects).
        if (who.ActiveObject is not null)
            return false;

        // Ask AI brain: should we override right now? Decide if we *want* to takeover
        if (!_ai.TryDecideTakeover(npc, who, location, out var takeover) || !takeover.ShouldTakeOver)
            return false;

        string context = 
            $"Season: {Game1.currentSeason}. " +
            $"Time: {Game1.timeOfDay}. " +
            $"Location: {location?.Name}. " +
            $"Weather: {Game1.isRaining}.";
        _dialogueService.StartNpcDialogue(npc, playerIntent: "Talk normally.", gameContext: context);
        return true;
    }

    private static void ShowDialogue(NPC npc, string text)
    {
        var dialogue = new Stack<Dialogue>();
        dialogue.Push(new Dialogue(npc, "", text));
        npc.TemporaryDialogue = dialogue;
        Game1.drawDialogue(npc);
    }
}
