
using Microsoft.Xna.Framework;

namespace GarettMValley.Agent.Memory;

/// <summary>
/// Represents an Agents long term memory in Stardew Valley.
/// </summary>
internal sealed class LongTermMemory
{
    /// <summary>
    /// Mappings of likes/dislikes to this thing/tag
    /// </summary>
    public Dictionary<string, Preference> PreferencesByTag { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Mappings of trust/like of actors/entities
    /// </summary>
    public Dictionary<long, Affinity> AffinityByTargetId { get; } = new();

    /// <summary>
    /// Mapping of facts this agent knows
    /// </summary>
    public Dictionary<string, string> Facts { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Valence baseline delta
    /// </summary>
    public float BaselineValence { get; set; } = 0f;

    /// <summary>
    /// Arousal baseline delta
    /// </summary>
    public float BaselineArousal { get; set; } = 0f;

    /// <summary>
    /// Stress baseline delta
    /// </summary>
    public float BaselineStress { get; set; } = 0f;

    /// <summary>
    /// Absorb an episode into our long-term memory storage
    /// </summary>
    /// <param name="episode"></param>
    public void Absorb(Episode episode)
    {
        if (episode.Kind.Equals("GotGift", StringComparison.OrdinalIgnoreCase))
        {
            if (episode.TargetId is long giver)
                AdjustAffinity(giver, delta: +0.05f);

            if (!string.IsNullOrWhiteSpace(episode.Tag))
                AdjustPreference(episode.Tag!, delta: +0.05f);
        }
        else if (episode.Kind.Equals("GotHit", StringComparison.OrdinalIgnoreCase))
        {
            if (episode.TargetId is long attacker)
                AdjustAffinity(attacker, delta: -0.10f);

            if (!string.IsNullOrWhiteSpace(episode.Tag))
                AdjustPreference(episode.Tag!, delta: -0.05f);
        }
        else if (episode.Kind.Equals("SawMonster", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(episode.Tag))
                AdjustPreference(episode.Tag!, delta: -0.02f); // fear/avoidance signal
        }
    }

    /// <summary>
    /// Adjust our preference in memory
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="delta"></param>
    public void AdjustPreference(string tag, float delta)
    {
        if (!PreferencesByTag.TryGetValue(tag, out var pref))
            pref = new Preference();

        pref.Value = MathHelper.Clamp(pref.Value + delta, -1f, 1f);
        pref.LastUpdatedUtc = DateTime.UtcNow;
        PreferencesByTag[tag] = pref;
    }

    /// <summary>
    /// Adjust our affinity in memory
    /// </summary>
    /// <param name="targetId"></param>
    /// <param name="delta"></param>
    public void AdjustAffinity(long targetId, float delta)
    {
        if (!AffinityByTargetId.TryGetValue(targetId, out var aff))
            aff = new Affinity();

        aff.Value = MathHelper.Clamp(aff.Value + delta, -1f, 1f);
        aff.LastUpdatedUtc = DateTime.UtcNow;
        AffinityByTargetId[targetId] = aff;
    }

    /// <summary>
    /// Get our preference to a given tag
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public float GetPreference(string tag)
        => PreferencesByTag.TryGetValue(tag, out var preference) ? preference.Value : 0f;

    /// <summary>
    /// Get our affinity to a given target
    /// </summary>
    /// <param name="targetId"></param>
    /// <returns></returns>
    public float GetAffinity(long targetId)
        => AffinityByTargetId.TryGetValue(targetId, out var affinity) ? affinity.Value : 0f;
}

/// <summary>
/// How the agent feels about a thing/label
/// </summary>
internal struct Preference
{
    /// <summary>
    /// How much we like something in the range: [-1.0, 1.0] (hate to love-ratio).
    /// </summary>
    public float Value;

    /// <summary>
    /// Last time this preference was updated.
    /// </summary>
    public DateTime LastUpdatedUtc;
}

/// <summary>
/// How the agent aligns with a person/thing.
/// </summary>
internal struct Affinity
{
    /// <summary>
    /// How much we align ourselfs with/trust someone or something in the range: [-1.0, 1.0] (distrust to trust-ratio).
    /// </summary>
    public float Value;

    /// <summary>
    /// Last time this affinity was updated.
    /// </summary>
    public DateTime LastUpdatedUtc;
}