using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace GarettMValley.AI;

/// <summary>
/// Agent AI Stimulus Bus
/// </summary>
public sealed class StimulusBus
{
    private readonly Dictionary<string, List<Stimulus>> _byLocation = new();
    private int _generation;

    public void Publish(Stimulus stimulus)
    {
        if (!_byLocation.TryGetValue(stimulus.locationName, out var list))
        {
            list = new();
            _byLocation[stimulus.locationName] = list;
        }
        
        // Mark stimulus with current generation
        var timestampedStimulus = stimulus;
        list.Add(timestampedStimulus);
    }

    public IEnumerable<Stimulus> ReadRecent(string locationName, int maxAgeTicks = 60)
    {
        if (!_byLocation.TryGetValue(locationName, out var list))
            yield break;
        
        foreach (var stimulus in list)
        {
            yield return stimulus;
        }
    }

    public void ClearLocation(string? locationName)
    {
        if (locationName is null)
            return;
        _byLocation.Remove(locationName);
    }

    public int DebugCountForLocation(string locationName)
    {
        if (_byLocation.TryGetValue(locationName, out var list))
            return list.Count;
        return 0;
    }
    
    /// <summary>
    /// Pulse generation forward (should be called periodically to clean old stimuli)
    /// </summary>
    public void NewGeneration()
    {
        _generation++;
        // Locations automatically expire old data when accessed
    }
}