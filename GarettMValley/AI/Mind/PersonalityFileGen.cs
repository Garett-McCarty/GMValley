using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Characters;
using GarettMValley.AI.Mind.Emotion;
using GarettMValley.AI.Mind.Personality;
using Microsoft.Xna.Framework;

namespace GarettMValley.AI.Mind;

public sealed class PersonalityFileGen
{
    private readonly IModHelper _helper;
    private readonly IMonitor _log;

    public PersonalityFileGen(IModHelper helper, IMonitor log)
    {
        _helper = helper;
        _log = log;
    }

    public void GenerateFile(string outPath = "assets/personalities.gen.json", bool force = false)
    {
        if (!force)
        {
            var existing = _helper.Data.ReadJsonFile<Dictionary<string, PersonalityProfile>>(outPath);
            if (existing is not null && existing.Count > 0)
                return;
        }

        var result = new Dictionary<string, PersonalityProfile>(StringComparer.OrdinalIgnoreCase);
        result["default"] = new PersonalityProfile
        {
            Key = "default",
            EmotionalHalfLife = 18.0f,
            Sociability = 0.5f,
            Bravery = 0.5f,
            Curiosity = 0.5f,
            Neuroticism = 0.5f,
            Dominance = 0.5f,
            ThreatSensitivity = 1.0f,
            SocialSensitivity = 1.0f,
            NoveltySensitivity = 1.0f,
            Baseline = EmotionalState.Neutral
        };

        Dictionary<string, CharacterData> characters;
        try
        {
            characters = _helper.GameContent.Load<Dictionary<string, CharacterData>>("Data/Characters");
        } catch (Exception exception)
        {
            _log.Log($"Failed to load Data/Characters: {exception}", LogLevel.Error);
            return;
        }
        
        foreach (var (key, data) in characters)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;

            float sociability = data.SocialAnxiety == NpcSocialAnxiety.Outgoing ? 0.80f : data.SocialAnxiety == NpcSocialAnxiety.Shy ? 0.30f : 0.55f + (data.CanBeRomanced ? 0.05f : 0.00f);
            float valence = data.Optimism == NpcOptimism.Positive ? 0.15f : data.Optimism == NpcOptimism.Negative ? -0.10f : 0.05f;
            float socialSensitivity = data.Manner == NpcManner.Rude ? 0.85f : data.Manner == NpcManner.Polite ? 1.15f : 1.00f;
            float dominance = data.Age == NpcAge.Adult ? 0.55f : data.Age == NpcAge.Teen ? 0.45f : data.Age == NpcAge.Child ? 0.35f : 0.5f;

            EmotionalState baseline = EmotionalState.Neutral;
            baseline.Valence = valence;
            baseline.Dominance = dominance;
            baseline.Normalize();
            
            PersonalityProfile personalityProfile = new PersonalityProfile
            {
                Key = key,
                EmotionalHalfLife = 18.0f,
                Sociability = sociability,
                Bravery = 0.5f,
                Curiosity = 0.5f,
                Neuroticism = 0.5f,
                Dominance = MathHelper.Clamp(dominance, 0.0f, 1.0f),
                ThreatSensitivity = 1.0f,
                SocialSensitivity = socialSensitivity,
                NoveltySensitivity = 1.0f,
                Baseline = baseline,
            };

            result[key] = personalityProfile;
        }

        _helper.Data.WriteJsonFile(outPath, result);
        _log.Log($"Wrote: {result.Count} personality profiles to {outPath}", LogLevel.Info);
    }
}