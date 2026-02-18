using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using System.Text;

namespace GarettMValley.AI;

internal sealed class DialogueService
{
    /// <summary>
    /// Reference to our DialogueHistory
    /// </summary>
    private readonly DialogueHistory _dialogueHistory;

    /// <summary>
    /// Reference to our DialogueSystem
    /// </summary>
    private readonly DialogueSystem _dialogue;

    /// <summary>
    /// Reference to the monitoring system
    /// </summary>
    private readonly IMonitor _monitor;
    /// <summary>
    /// Reference to the thread dispatcher
    /// </summary>
    private readonly MainThreadDispatcher _dispatcher;
    /// <summary>
    /// Reference to our task queue
    /// </summary>
    private readonly SingleFlightQueue _queue;
    /// <summary>
    /// Reference to our OllamaClient wrapper
    /// </summary>
    private readonly OllamaClient _ollama;
    /// <summary>
    /// Model we want to use with our OllamaClient
    /// </summary>
    private readonly string _model;

    public DialogueService(IMonitor monitor, MainThreadDispatcher dispatcher, string ollamaUri, string ollamaModel, int timeoutSeconds, DialogueSystem dialogue)
    {
        _dialogue = dialogue;
        _dialogueHistory = new DialogueHistory(maxTurns: 16);
        _monitor = monitor;
        _dispatcher = dispatcher;
        _queue = new SingleFlightQueue(monitor);
        _ollama = new OllamaClient(baseUrl: ollamaUri, timeout: TimeSpan.FromSeconds(timeoutSeconds));
        _model = ollamaModel;
    }

    public void Dispose()
    {
        _queue.Dispose();
        _ollama.Dispose();
    }

    public void CancelActive()
    {
        _queue.CancelCurrent();
    }

    public void StartNpcDialogue(NPC npc, Func<string> getSystemPrompt, Func<string, string> getUserPrompt, Func<string> getNpcName)
    {
        StartNpcDialogue(npc, getSystemPrompt, getUserPrompt, getNpcName, playerSaid: "Talk normally.");
    }

    public void StartNpcDialogue(NPC npc, Func<string> getSystemPrompt, Func<string, string> getUserPrompt, Func<string> getNpcName, string playerSaid)
    {
        string npcName = getNpcName();

        _queue.ReplaceCurrent(async token =>
        {
            string systemPrompt = getSystemPrompt();
            StringBuilder userPromptBuilder = new StringBuilder();
            userPromptBuilder.AppendLine(_dialogueHistory.BuildTranscript(npcName));
            userPromptBuilder.AppendLine();
            userPromptBuilder.AppendLine(getUserPrompt(playerSaid));
            string userPrompt = userPromptBuilder.ToString();

            var options = new Dictionary<string, object>
            {
                ["temperature"] = 0.8,
                ["top_p"] = 0.9,
                ["num_predict"] = 160
            };

            // Off-thread network call
            var result = await _ollama.GenerateAsync(
                model: _model,
                prompt: userPrompt,
                system: systemPrompt,
                options: options,
                cancellationToken: token
            ).ConfigureAwait(false);

            string buf = result.Text;
            if (!DialogueJsonParser.TryParse(buf, out var parsed))
            {
                parsed = new DialogueJsonModel
                {
                    NpcLine = "...",
                    PlayerOptions = new[] { "Hey!", "How's it going?", "Bye." },
                    Intent = "smalltalk",
                    Tags = Array.Empty<string>(),
                };
            }
            
            string npcLine = parsed.NpcLine ?? "...";
            string[] dialogueOptions = parsed.PlayerOptions ?? new[] { "Hey!", "How's it going?", "Bye." };

            // Marshal back to the main thread to touch Stardew UI/state
            _dispatcher.Enqueue(() =>
            {
                if (Game1.currentLocation is null)
                    return;
                
                // TODO: Allow agent the ability to not to talk if its not in the mood.
                
                _dialogueHistory.AddNpcLine(npcName, npcLine);

                DialogueUI.ShowLineThenOptions(npc: npc, location: Game1.currentLocation, line: npcLine, playerOptions: dialogueOptions, onPicked: (idx, chosenText) =>
                {
                    _dialogueHistory.AddPlayerLine(npcName, chosenText);
                    if (chosenText != "Bye.")
                    {
                        StartNpcDialogue(npc, getSystemPrompt, getUserPrompt, getNpcName, chosenText);
                    }
                });
            });
        });
    }

    private static string SanitizeDialogue(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return String.Empty;
        var value = text.Trim();
        if (value.Length > 240)
            value = value.Substring(0, 240).TrimEnd() + "…";
        value = value.Replace("\r", " ").Replace("\n", " ").Trim();
        return value;
    }
}