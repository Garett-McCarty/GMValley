using System;
using System.Collections.Generic;
using System.Text;

namespace GarettMValley.AI;

internal sealed class DialogueHistory
{
    public readonly record struct DialogueAction(string speaker, string text);

    private readonly int _maxTurns;
    private readonly Dictionary<string, Deque<DialogueAction>> _bySpeaker = new();

    public DialogueHistory(int maxTurns = 8)
    {
        _maxTurns = maxTurns;
    }

    /// <summary>
    /// Add a NPC line to the dialogue history
    /// </summary>
    /// <param name="npcName"></param>
    /// <param name="npcLine"></param>
    public void AddNpcLine(string npcName, string npcLine) => Add(npcName, new DialogueAction("npc", npcLine));

    /// <summary>
    /// Add a Player line to the dialogue history
    /// </summary>
    /// <param name="npcName"></param>
    /// <param name="playerLine"></param>
    public void AddPlayerLine(string npcName, string playerLine) => Add(npcName, new DialogueAction("player", playerLine));

    /// <summary>
    /// Build a transcript of the recent dialogue history for our prompt later
    /// </summary>
    /// <param name="npcName"></param>
    /// <returns></returns>
    public string BuildTranscript(string npcName)
    {
        if (!_bySpeaker.TryGetValue(npcName, out var deque) || deque.Count == 0)
            return "";
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("RECENT_CONVERSATION:");
        foreach(var line in deque.Items)
        {
            stringBuilder.AppendLine($"{line.speaker}: {line.text}");
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Add a NPC dialogue action to memory
    /// </summary>
    /// <param name="npcName"></param>
    /// <param name="dialogueAction"></param>
    private void Add(string npcName, DialogueAction dialogueAction)
    {
        if (string.IsNullOrWhiteSpace(npcName))
            return;
        if (!_bySpeaker.TryGetValue(npcName, out var deque))
        {
            deque = new Deque<DialogueAction>(_maxTurns);
            _bySpeaker[npcName] = deque;
        }
        deque.PushBack(dialogueAction);
        while (deque.Count > _maxTurns)
            deque.PopFront();
    }

    /// <summary>
    /// Tiny deque without LINQ allocations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private sealed class Deque<T>
    {
        private readonly LinkedList<T> _list = new();
        private readonly int _capacity;
        
        public Deque(int capacity)
        {
            _capacity = capacity;
        }

        public int Count => _list.Count;
        public IEnumerable<T> Items => _list;

        public void PushBack(T item) => _list.AddLast(item);
        public void PopFront()
        {
            if (_list.First != null)
            {
                _list.RemoveFirst();
            }
        }
    }
}