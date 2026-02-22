
namespace GarettMValley.Agent.Memory;

/// <summary>
/// Represents an Agents working memory in Stardew Valley.
/// </summary>
internal sealed class MemoryStore
{
    /// <summary>
    /// Agents episodic memory
    /// </summary>
    public EpisodicMemory episodic;

    /// <summary>
    /// Agents long term memory 
    /// </summary>
    public LongTermMemory longTerm;
    
    /// <summary>
    /// Construct an Agent memory storage.
    /// </summary>
    /// <param name="episodic"></param>
    /// <param name="longTerm"></param>
    public MemoryStore(EpisodicMemory episodic, LongTermMemory longTerm)
    {
        this.episodic = episodic;
        this.longTerm = longTerm;
    }

    /// <summary>
    /// Tick the episodic memory forward in time.
    /// </summary>
    /// <param name="dateTime"></param>
    public void Tick(DateTime dateTime)
    {
        episodic.TrimExpired(dateTime);
    }

    /// <summary>
    /// Record an event and immediately fold it into long-term if desired.
    /// </summary>
    /// <param name="episode"></param>
    public void Record(Episode episode)
    {
        episodic.Add(episode);
        longTerm.Absorb(episode);
    }
}