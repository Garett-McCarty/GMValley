using System;
using System.Collections.Generic;
using System.IO;
using StardewModdingAPI;
using StardewValley.GameData.FishPonds;

namespace GarettMValley.Agent.Mind.Personality;

public sealed class PersonalityLibrary
{
    private readonly Dictionary<string, PersonalityProfile> _profiles;
    private readonly Dictionary<string, string> _profilePrompts = new();
    private readonly PersonalityProfile _fallback;
    private readonly IModHelper _helper;

    public PersonalityLibrary(IModHelper helper)
    {
        _helper = helper;
        _profiles = helper.Data.ReadJsonFile<Dictionary<string, PersonalityProfile>>("assets/personalities.json") ?? new Dictionary<string, PersonalityProfile>();

        string? fileName = null;
        string? prompt = null;
        foreach(var key in _profiles.Keys)
        {
            if (key == "default")
                continue;
            
            fileName = $"assets/prompts/{FormatNameForFile(key)}.md";
            try
            {
                string fullPath = Path.Combine(helper.DirectoryPath, fileName);
                if (File.Exists(fullPath))
                {
                    prompt = File.ReadAllText(fullPath);
                    if (prompt != null)
                        _profilePrompts[key] = prompt;
                }
            }
            catch (Exception)
            {
                // Silently skip loading prompts if file reading fails
            }
        }
        _fallback = _profiles.TryGetValue("default", out var def) ? def : new PersonalityProfile { Key = "default" };
    }

    public PersonalityProfile GetForCharacterName(string name) => _profiles.TryGetValue(name, out var pp) ? pp : _fallback;
    public string GetPromptForCharacterName(string name) => _profilePrompts.TryGetValue(name, out var prompt) ? prompt : string.Empty;

    private static string FormatNameForFile(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
    
        // Replace any whitespace with underscore
        return System.Text.RegularExpressions.Regex.Replace(input, @"\s+", "_").ToLowerInvariant();
    }
}