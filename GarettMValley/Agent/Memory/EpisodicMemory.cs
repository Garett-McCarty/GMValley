
namespace GarettMValley.Agent.Memory;

/// <summary>
/// Represents an Agents short term (episodic) memory in Stardew Valley.
/// </summary>
internal sealed class EpisodicMemory
{
    /// <summary>
    /// Ring buffer of recent events
    /// </summary>
    private readonly Episode[] _buffer;

    /// <summary>
    /// Count of events recorded
    /// </summary>
    private int _count;
    
    /// <summary>
    /// Next write index
    /// </summary>
    private int _head;
    
    /// <summary>
    /// Capacity of this episodic memory
    /// </summary>
    public int Capacity => _buffer.Length;

    /// <summary>
    /// Count of the episodes recorded in memory
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Optionally expire episodes past/older than MaxAge.
    /// </summary>
    public TimeSpan? MaxAge { get; set; } = TimeSpan.FromHours(6);

    /// <summary>
    /// Construct an episodic memory store with a given capacity.
    /// </summary>
    /// <param name="capacity"></param>
    public EpisodicMemory(int capacity = 64)
    {
        if (capacity < 8)
            capacity = 8;
        _buffer = new Episode[capacity];
    }

    /// <summary>
    /// Add a memory episode into our episodic/short-term memory.
    /// </summary>
    /// <param name="episode"></param>
    public void Add(Episode episode)
    {
        _buffer[_head] = episode;
        _head = (_head + 1) % _buffer.Length;
        if (_count < _buffer.Length)
            _count += 1;
    }

    /// <summary>
    /// Trim expired events from our episodic memory.
    /// </summary>
    /// <param name="dateTime"></param>
    public void TrimExpired(DateTime dateTime)
    {
        if (MaxAge is null || _count == 0)
            return;
        var cutoff = dateTime - MaxAge.Value;
        var buf = new List<Episode>();
        foreach(var episode in EnumerateNewestFirst())
        {
            if (episode.DateTime < cutoff)
                break;
            buf.Add(episode);
        }
        _count = 0;
        _head = 0;
        for (int i = buf.Count - 1; i >= 0; i--)
            Add(buf[i]);
    }

    /// <summary>
    /// Enumerate through the episodic memory, retrieving the newest first and walking back in time.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Episode> EnumerateNewestFirst()
    {
        for (int i = 0; i < _count; i++)
        {
            int idx = _head - 1 - i;
            if (idx < 0) idx += _buffer.Length;
            yield return _buffer[idx];
        }
    }

    /// <summary>
    /// Find a recent memory in our episodic memory store.
    /// </summary>
    /// <param name="predicate">Search function</param>
    /// <param name="max">How deep we want to search through our storage pool.</param>
    /// <returns></returns>
    public IEnumerable<Episode> FindRecent(Func<Episode, bool> predicate, int max = 10)
    {
        int found = 0;
        foreach (var episode in EnumerateNewestFirst())
        {
            if (predicate(episode))
            {
                yield return episode;
                if (++found >= max)
                    yield break;
            }
        }
    }
}