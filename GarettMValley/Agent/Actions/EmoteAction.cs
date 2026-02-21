using GarettMValley.Agent.Mind;
using GarettMValley.Agent.Emote;

namespace GarettMValley.Agent.Actions;

/// <summary>
/// Emote (Expression) action for agents
/// </summary>
public sealed class EmoteAction : IAction
{
    /// <summary>
    /// Emote intent key
    /// </summary>
    public string IntentKey => "emote";

    /// <summary>
    /// Determine if an Emote is finished
    /// </summary>
    public bool IsFinished { get; private set; } = false;

    /// <summary>
    /// Score the weight of this agent wanting to emote.
    /// </summary>
    /// <param name="mindstate"></param>
    /// <returns></returns>
    public float Score(MindState mindstate)
    {
        return mindstate.PlayerIsNear && !mindstate.IsOnCooldown("emote") ? 0.35f : 0.0f;
    }

    /// <summary>
    /// Determine if the emote can be continued.
    /// </summary>
    /// <param name="_mindstate"></param>
    /// <returns></returns>
    public bool CanContinue(MindState _mindstate) => true;

    /// <summary>
    /// Start the Emote action
    /// </summary>
    /// <param name="_minestate"></param>
    public void Start(MindState _minestate) { IsFinished = false; }

    /// <summary>
    /// Tick the action forward in time
    /// </summary>
    /// <param name="mindstate"></param>
    public void Tick(MindState mindstate)
    { 
        int emote = EmotePicker.Pick(mindstate.Emotion, mindstate.ThreatNearby, EmoteContext.Idle);
        mindstate.Self!.Emote(emote);
        mindstate.SetCooldown(IntentKey, 120); // ~2 seconds if tick interval is fast,
        IsFinished = true;
    }

    /// <summary>
    /// Abort the emote action
    /// </summary>
    /// <param name="_mindstate"></param>
    public void Abort(MindState _mindstate) { IsFinished = true; }
}