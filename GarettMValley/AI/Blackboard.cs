using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GarettMValley.AI.Mind;
using Microsoft.Xna.Framework;
using StardewValley;

namespace GarettMValley.AI;

/// <summary>
/// Agent State (stores variables needed for AI systems)
/// </summary>
public sealed class Blackboard
{
    /// <summary>
    /// Current location in the game world
    /// </summary>
    [AllowNull]
    public GameLocation Location { get; private set; } = null;

    /// <summary>
    /// Current agent adapter used by the agent
    /// </summary>
    [AllowNull]
    public IAgentAdapter Self { get; private set; } = null;

    /// <summary>
    /// Reference to the player
    /// </summary>
    [AllowNull]
    public Farmer Player { get; private set; } = null;

    /// <summary>
    /// Reference to the stimulus bus
    /// </summary>
    [AllowNull]
    public StimulusBus Stimuli { get; private set; } = null;

    /// <summary>
    /// Distance to player from this agent in tiles
    /// </summary>
    public float DistanceToPlayerTiles { get; set; }

    /// <summary>
    /// Flag to determine if the player is near this agent.
    /// </summary>
    public bool PlayerIsNear { get; set; }

    /// <summary>
    /// Check if threats are near
    /// </summary>
    public bool ThreatNearby { get; set; }

    /// <summary>
    /// Threat location
    /// </summary>
    public Vector2 ThreatTile { get; set; }
    /// <summary>
    /// Count of nearby agents
    /// </summary>
    public int NearbyAgentsCount { get; set; }

    /// <summary>
    /// The minds emotional state
    /// </summary>
    public EmotionalState Emotion = EmotionalState.Neutral;

    /// <summary>
    /// The minds personality profile
    /// </summary>
    public PersonalityProfile Personality = new();

    /// <summary>
    /// The agents intent (current goal)
    /// </summary>
    public string? IntentKey { get; private set; }

    /// <summary>
    /// The agents intent tick timer (attention span)
    /// </summary>
    public int IntentTicksLeft { get; private set; }
    /// <summary>
    /// Determine if the agent has or is in an IAction
    /// </summary>
    public bool HasIntent => IntentTicksLeft > 0 && IntentKey is not null;
    /// <summary>
    /// The identifier of the nearest friendly entity
    /// </summary>
    public string? NearestFriendlyId { get; set; }
    /// <summary>
    /// The tile location of the nearest friendly entity
    /// </summary>
    public Vector2 NearestFriendlyTile { get; set; }
    /// <summary>
    /// Distance to the nearest friendly entity in tiles
    /// </summary>
    public float DistToNearestFriendlyTiles { get; set; } = float.MaxValue;

    /// <summary>
    /// Reference to the agents adapter
    /// </summary>
    public object SelfRaw => Self.Raw;

    /// <summary>
    /// The target tile the agent wants to move to
    /// </summary>
    public Vector2? MoveTargetTile { get; private set; }

    /// <summary>
    /// Dictionary of event cooldowns. Mapped as [action_key => cooldown_ticks]
    /// </summary>
    private readonly Dictionary<string, int> _cooldowns = new();
    
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
    public bool IsOnCooldown(string key) => _cooldowns.TryGetValue(key, out var t) && t > 0;

    /// <summary>
    /// Set an action on cooldown until a given ticks is reached.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="ticks"></param>
    public void SetCooldown(string key, int ticks) => _cooldowns[key] = ticks;

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
        var keys = _cooldowns.Keys.ToArray();
        foreach (var key in keys)
        {
            _cooldowns[key] = Math.Max(0, _cooldowns[key] - 1);
        }
    }
}