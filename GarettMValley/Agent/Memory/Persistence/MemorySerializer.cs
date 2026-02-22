using System.Linq;

namespace GarettMValley.Agent.Memory.Persistence;

internal static class MemorySerializer
{
    /// <summary>
    /// Convert our memory store into a JSON-safe AgentMemoryData container.
    /// </summary>
    /// <param name="store"></param>
    /// <returns></returns>
    public static AgentMemoryData ToData(MemoryStore store)
    {
        return new AgentMemoryData
        {
            Episodic = store.episodic.EnumerateNewestFirst().Reverse().ToList(),
            LongTerm = new LongTermData
            {
                PreferencesByTag = new(store.longTerm.PreferencesByTag),
                AffinityByTargetId = new(store.longTerm.AffinityByTargetId),
                Facts = new(store.longTerm.Facts),
                BaselineValence = store.longTerm.BaselineValence,
                BaselineArousal = store.longTerm.BaselineArousal,
                BaselineStress = store.longTerm.BaselineStress,
            }
        };
    }

    /// <summary>
    /// Convert our JSON-friendly AgentMemoryData container to a new MemoryStore.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="episodicCapacity"></param>
    /// <returns></returns>
    public static MemoryStore FromData(AgentMemoryData data, int episodicCapacity = 64)
    {
        var episodic = new EpisodicMemory(episodicCapacity);
        foreach (var episode in data.Episodic)
            episodic.Add(episode);
        var longTerm = new LongTermMemory
        {
            BaselineValence = data.LongTerm.BaselineValence,
            BaselineArousal = data.LongTerm.BaselineArousal,
            BaselineStress = data.LongTerm.BaselineStress,
        };
        foreach (var (Key, Value) in data.LongTerm.PreferencesByTag)
            longTerm.PreferencesByTag[Key] = Value;
        foreach (var (Key, Value) in data.LongTerm.AffinityByTargetId)
            longTerm.AffinityByTargetId[Key] = Value;
        foreach (var (Key, Value) in data.LongTerm.Facts)
            longTerm.Facts[Key] = Value;
        
        return new MemoryStore(episodic, longTerm);
    }
}