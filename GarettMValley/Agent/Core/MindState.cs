using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GarettMValley.Agent.Mind.Emotion;
using GarettMValley.Agent.Mind.Personality;
using GarettMValley.Agent.Perception;
using GarettMValley.Agent.Stimuli;
using Microsoft.Xna.Framework;
using StardewValley;

namespace GarettMValley.Agent.Core;

/// <summary>
/// Stores all the necessary state variables of an Agent.
/// </summary>
public sealed class MindState
{
    /// <summary>
    /// Map of agent event cooldowns, Mapped as [action key => cooldown ticks in seconds].
    /// </summary>
    private readonly Dictionary<string, int> _eventCooldowns = new();

    /// <summary>
    /// Agents current location
    /// </summary>
    public GameLocation? Location { get; private set; } = null;

    /// <summary>
    /// Agents current adapter controller
    /// </summary>
    public IAgentAdapter? Self { get; private set; } = null;

    /// <summary>
    /// Current player
    /// </summary>
    public Farmer? Player { get; private set; } = null;

    /// <summary>
    /// Agents Stimulus Bus
    /// </summary>
    public StimulusBus? Stimuli { get; private set; } = null;

    /// <summary>
    /// Agents emotional state
    /// </summary>
    public EmotionalState Emotion = EmotionalState.Neutral;

    /// <summary>
    /// Agents personality profile
    /// </summary>
    public PersonalityProfile Personality = new();

    /// <summary>
    /// Visually perceived events
    /// </summary>
    public Dictionary<string, PerceivedEntity> VisuallyPerceived { get; } = new();

    /// <summary>
    /// Throttle control for visual stimulus
    /// </summary>
    public Dictionary<string, int> LastVisualStimulusTickByTarget { get; } = new();

    /// <summary>
    /// Distance to player in tiles
    /// </summary>
    public float DistanceToPlayerTiles { get; set; } = float.NegativeInfinity;

    /// <summary>
    /// Determine if the player is near
    /// </summary>
    public bool PlayerIsNear { get; set; } = false;

    public float PlayerSee { get; set; } = 0.0f;
    public float PlayerAwareness { get; set; } = 0.0f;

    /// <summary>
    /// Determine if threats are near
    /// </summary>
    public bool ThreatNearby { get; set; }

    /// <summary>
    /// Threat location tile
    /// </summary>
    public Vector2 ThreatTile { get; set; }

    /// <summary>
    /// Identifier of the nearest friendly entity
    /// </summary>
    public string? NearestFriendlyId { get; set; }

    /// <summary>
    /// The tile location of the nearest friendly entity
    /// </summary>
    public Vector2 NearestFriendlyTile { get; set; }
    
    /// <summary>
    /// Distance to the nearest friendly entity in tiles
    /// </summary>
    public float DistanceToNearestFriendly { get; set; } = float.MaxValue;

    /// <summary>
    /// Count of all nearby agents
    /// </summary>
    public int NearbyAgentsCount { get; set; }

    /// <summary>
    /// Agents intent (current goal)
    /// </summary>
    public string? IntentKey { get; private set; }

    /// <summary>
    /// Agents intent tick timer (attention span)
    /// </summary>
    public int IntentTicksLeft { get; private set; }

    /// <summary>
    /// Determine if the agent has an active Intent
    /// </summary>
    public bool HasIntent => IntentTicksLeft > 0 && IntentKey is not null;

    /// <summary>
    /// Reference to the agents adapter
    /// </summary>
    public object SelfRaw => Self!.Raw;

    /// <summary>
    /// Target tile the agent wants to move to
    /// </summary>
    public Vector2? MoveTargetTile { get; private set; }
    
    /// <summary>
    /// Current tick accumulator
    /// </summary>
    private int _tick;

    /// <summary>
    /// Apply EmotionDelta to our Emotion state
    /// </summary>
    /// <param name="delta"></param>
    public void ApplyDelta(EmotionDelta delta) => Emotion.Apply(delta);

    /// <summary>
    /// Begin game tick logic
    /// </summary>
    /// <param name="location"></param>
    /// <param name="self"></param>
    /// <param name="player"></param>
    /// <param name="stimuli"></param>
    public void BeginTick(GameLocation location, IAgentAdapter self, Farmer player, StimulusBus stimuli)
    {
        _tick += 1;
        Location = location;
        Self = self;
        Player = player;
        Stimuli = stimuli;

        PlayerIsNear = false;
        ThreatNearby = false;
        NearbyAgentsCount = 0;
        DistanceToPlayerTiles = Vector2.Distance(Self.Tile, Player.Tile);

        foreach (var pair in VisuallyPerceived)
            pair.Value.SeenThisTick = false;
        PlayerSee = 0.0f;

        if (IntentTicksLeft > 0)
            IntentTicksLeft -= 1;
        if (IntentTicksLeft <= 0)
            ClearIntent();

        TickCooldowns();
    }

    /// <summary>
    /// Get the current tick value
    /// </summary>
    public int Tick => _tick;

    /// <summary>
    /// Check if a given action is in cooldown
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsOnCooldown(string key) => _eventCooldowns.TryGetValue(key, out var t) && t > 0;

    /// <summary>
    /// Set an action on cooldown until a given ticks is reached.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="ticks"></param>
    public void SetCooldown(string key, int ticks) => _eventCooldowns[key] = ticks;

    /// <summary>
    /// Start a given intent
    /// </summary>
    /// <param name="key"></param>
    /// <param name="ticks"></param>
    public void StartIntent(string key, int ticks)
    {
        IntentKey = key;
        IntentTicksLeft = Math.Max(1, ticks);
    }

    /// <summary>
    /// Clear the intent we are working with
    /// </summary>

    public void ClearIntent()
    {
        IntentKey = null;
        IntentTicksLeft = 0;
    }

    /// <summary>
    /// Set the target of where the agent wants to move to
    /// </summary>
    /// <param name="tile"></param>
    public void SetMoveTarget(Vector2 tile) => MoveTargetTile = tile;

    /// <summary>
    /// Clear the target of where the agents wants to move to
    /// </summary>
    public void ClearMoveTarget() => MoveTargetTile = null;

    /// <summary>
    /// Helper to prevent spamming of actions
    /// </summary>
    /// <param name="key"></param>
    /// <param name="ticks"></param>
    /// <returns>True if we set the action in cooldown, False if its otherwise in cooldown.</returns>
    public bool TryPulseCooldown(string key, int ticks)
    {
        if (IsOnCooldown(key))
            return false;
        SetCooldown(key, ticks);
        return true;
    }

    /// <summary>
    /// Handle cooldown ticks for actions
    /// </summary>
    private void TickCooldowns()
    {
        var keys = _eventCooldowns.Keys.ToArray();
        foreach (var key in keys)
        {
            _eventCooldowns[key] = Math.Max(0, _eventCooldowns[key] - 1);
        }
    }
}