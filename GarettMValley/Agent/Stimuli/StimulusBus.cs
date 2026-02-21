
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Stimuli;

/// <summary>
/// Stimulus Bus for Agents to react too
/// </summary>
public sealed class StimulusBus
{
    private sealed class Entry
    {
        public IStimulus Stimulus = null!;
        public LinkedListNode<Entry> Node = null!;
        public string? LocationName;
        public Point TilePoint;
        public Point Bucket;
    }

    /// <summary>
    /// First in/First out, linked list of Stimuli entries
    /// </summary>
    private readonly LinkedList<Entry> _fifo = new();

    /// <summary>
    /// Stimuli segmented by spatial index: Location -> bucket -> entries
    /// </summary>
    private readonly Dictionary<string, Dictionary<Point, HashSet<Entry>>> _spatial = new();

    /// <summary>
    /// Location index for fast clear
    /// </summary>
    private readonly Dictionary<string, HashSet<Entry>> _byLocation = new();

    /// <summary>
    /// Bucket size in tiles.
    /// </summary>
    public int BucketSizeTiles { get; }

    public StimulusBus(int bucketSizeTiles = 8)
    {
        BucketSizeTiles = Math.Max(1, bucketSizeTiles);
    }

    /// <summary>
    /// Publish stimulus to the StimulusBus
    /// </summary>
    /// <param name="stimulus"></param>
    public void Publish(IStimulus stimulus)
    {
        var entry = new Entry
        {
            Stimulus = stimulus,
            LocationName = stimulus.LocationName,
            TilePoint = ToTilePoint(stimulus.Tile),
            Bucket = ToBucket(ToTilePoint(stimulus.Tile)),
            Node = null!,
        };

        entry.Node = _fifo.AddLast(entry);

        if (!string.IsNullOrEmpty(entry.LocationName))
        {
            Index(entry);
        }
    }

    public bool TryDequeue(out IStimulus? stimulus)
    {
        if (_fifo.First is null)
        {
            stimulus = null;
            return false;
        }

        var entry = _fifo.First.Value;
        _fifo.RemoveFirst();
        if (!string.IsNullOrEmpty(entry.LocationName))
            Unindex(entry);
        stimulus = entry.Stimulus;
        return true;
    }

    public void Clear()
    {
        _fifo.Clear();
        _spatial.Clear();
        _byLocation.Clear();
    }

    public int DebugCount() => _fifo.Count;

    public int DebugCountByLocation(string locationName)
    {
        if (_byLocation.TryGetValue(locationName, out var set))
            return set.Count;
        return 0;
    }

    public int DebugCountByLocationAndKind(string locationName, StimulusKind? kind = null)
    {
        if (!_byLocation.TryGetValue(locationName, out var set))
            return 0;

        if (kind is null)
            return set.Count;

        int count = 0;
        foreach (var entry in set)
        {
            if (entry.Stimulus.StimulusKind == kind.Value)
                count++;
        }

        return count;
    }

    public Dictionary<string, int> DebugCountAllByLocation()
    {
        var result = new Dictionary<string, int>();
        foreach (var pair in _byLocation)
            result[pair.Key] = pair.Value.Count;
        return result;
    }

    public string DebugSummary()
    {
        var parts = new List<string>();
        foreach (var pair in _byLocation)
            parts.Add($"{pair.Key}: {pair.Value.Count}");
        if (parts.Count == 0)
            return "No spatial stimuli";
        return string.Join(" | ", parts);
    }

    public int ClearLocation(string locationName)
    {
        if (!_byLocation.TryGetValue(locationName, out var set))
            return 0;
        
        int removed = 0;
        foreach(var entry in set.ToArray())
        {
            _fifo.Remove(entry.Node);
            Unindex(entry);
            removed += 1;
        }

        return removed;
    }

    public int KeepOnlyLocation(string locationName)
    {
        int removed = 0;
        var locations = _byLocation.Keys.ToArray();
        foreach(var location in locations)
        {
            if (location != locationName)
            {
                removed += ClearLocation(location);
            }
        }

        return removed;
    }

    /// <summary>
    /// Query stimuli within a radius in tile units, from a location.
    /// </summary>
    /// <param name="locationName"></param>
    /// <param name="centerTile"></param>
    /// <param name="radiusTiles"></param>
    /// <param name="kind"></param>
    /// <param name="minIntensity"></param>
    /// <returns></returns>
    public List<IStimulus> QueryRadius(string locationName, Vector2 centerTile, float radiusTiles, StimulusKind? kind = null, float minIntensity = 0.0f)
    {
        var results = new List<IStimulus>();

        if (!_spatial.TryGetValue(locationName, out var buckets))
            return results;

        if (radiusTiles <= 0.0f)
            return results;
        
        var center = ToTilePoint(centerTile);
        float radiusSq = radiusTiles * radiusTiles;
        int bucketSize = BucketSizeTiles;
        int minBucketX = FloorDivision(center.X - (int)Math.Ceiling(radiusTiles), bucketSize);
        int maxBucketX = FloorDivision(center.X + (int)Math.Ceiling(radiusTiles), bucketSize);
        int minBucketY = FloorDivision(center.Y - (int)Math.Ceiling(radiusTiles), bucketSize);
        int maxBucketY = FloorDivision(center.Y + (int)Math.Ceiling(radiusTiles), bucketSize);

        for (int y = minBucketY; y <= maxBucketY; y++)
        {
            for (int x = minBucketX; x <= maxBucketX; x++)
            {
                var key = new Point(x, y);
                if (!buckets.TryGetValue(key, out var entries))
                    continue;
                foreach (var entry in entries)
                {
                    var stimulus = entry.Stimulus;
                    if (kind.HasValue && stimulus.StimulusKind != kind.Value)
                        continue;
                    if (stimulus.Intensity < minIntensity)
                        continue;
                    float dx = entry.TilePoint.X - center.X;
                    float dy = entry.TilePoint.Y - center.Y;
                    float z = (dx * dx) + (dy * dy);
                    if (z <= radiusSq)
                        results.Add(stimulus);
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Remove stimuli older than maxAgeTicks
    /// </summary>
    /// <param name="currentTick"></param>
    /// <param name="maxAgeTicks"></param>
    /// <returns></returns>
    public int PruneOld(int currentTick, int maxAgeTicks)
    {
        if (maxAgeTicks <= 0)
            return 0;
        int removed = 0;
        var node = _fifo.First;

        while (node is not null)
        {
            var next = node.Next;
            var entry = node.Value;
            if ((currentTick - entry.Stimulus.Tick) > maxAgeTicks)
            {
                _fifo.Remove(node);
                if (!string.IsNullOrEmpty(entry.LocationName))
                    Unindex(entry);
                removed++;
            }

            node = next;
        }

        return removed;
    }

    private void Index(Entry entry)
    {
        var location = entry.LocationName!;
        if (!_byLocation.TryGetValue(location, out var locationSet))
        {
            locationSet = new HashSet<Entry>();
            _byLocation[location] = locationSet;
        }
        locationSet.Add(entry);

        if (!_spatial.TryGetValue(location, out var buckets))
        {
            buckets = new Dictionary<Point, HashSet<Entry>>();
            _spatial[location] = buckets;
        }

        if (!buckets.TryGetValue(entry.Bucket, out var bucketSet))
        {
            bucketSet = new HashSet<Entry>();
            buckets[entry.Bucket] = bucketSet;
        }

        bucketSet.Add(entry);
    }

    private void Unindex(Entry entry)
    {
        var location = entry.LocationName!;
        if (_byLocation.TryGetValue(location, out var locationSet))
        {
            locationSet.Remove(entry);
            if (locationSet.Count == 0)
                _byLocation.Remove(location);
        }

        if (_spatial.TryGetValue(location, out var buckets) && buckets.TryGetValue(entry.Bucket, out var bucketSet))
        {
            bucketSet.Remove(entry);
            if (bucketSet.Count == 0)
                buckets.Remove(entry.Bucket);
            if (buckets.Count == 0)
                _spatial.Remove(location);
        }
    }

    private static Point ToTilePoint(Vector2 tile) => new((int)MathF.Floor(tile.X), (int)MathF.Floor(tile.Y));
    private Point ToBucket(Point tile) => new(FloorDivision(tile.X, BucketSizeTiles), FloorDivision(tile.Y, BucketSizeTiles));
    private static int FloorDivision(int a, int b)
    {
        int q = a / b;
        int r = a % b;
        if (r != 0 && ((r > 0) != (b > 0)))
            q--;
        return q;
    }
}