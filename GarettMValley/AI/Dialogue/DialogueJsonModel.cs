using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GarettMValley.AI;

internal sealed class DialogueJsonModel
{
    [JsonPropertyName("npc_line")]
    public string? NpcLine { get; set; }

    [JsonPropertyName("player_options")]
    public string[]? PlayerOptions { get; set; }

    [JsonPropertyName("intent")]
    public string? Intent { get; set; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }
}

internal static class DialogueJsonParser
{
    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static bool TryParse(string raw, out DialogueJsonModel parsed)
    {
        parsed = new DialogueJsonModel();
        if (string.IsNullOrWhiteSpace(raw))
            return false;
        
        string json = ExtractJsonObject(raw);
        if (string.IsNullOrWhiteSpace(json))
            return false;
        try
        {
            parsed = JsonSerializer.Deserialize<DialogueJsonModel>(json, serializerOptions) ?? new DialogueJsonModel();
        } catch
        {
            return false;
        }

        parsed.NpcLine = SanitizeLine(parsed.NpcLine);
        var options = parsed.PlayerOptions ?? Array.Empty<string>();
        options = options.Where(line => !string.IsNullOrWhiteSpace(line)).Select(SanitizeOption).ToArray();
        if (options.Length < 3)
        {
            var buf = new string[3];
            for (int i = 0; i < 3; i++)
                buf[i] = i < options.Length ? options[i] : DefaultOption(i);
            options = buf;
        }
        else if (options.Length > 3)
        {
            options = options.Take(3).ToArray();
        }
        parsed.PlayerOptions = options;
        parsed.Tags = (parsed.Tags ?? Array.Empty<string>()).Take(2).ToArray();
        parsed.Intent = string.IsNullOrWhiteSpace(parsed.Intent) ? "smalltalk" : parsed.Intent.Trim();

        return !string.IsNullOrWhiteSpace(parsed.NpcLine);
    }

    private static string ExtractJsonObject(string raw)
    {
        int start = raw.IndexOf('{');
        int end = raw.LastIndexOf('}');
        if (start < 0 || end <= start)
            return "";
        return raw.Substring(start, end - start + 1);
    }

    private static string SanitizeLine(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return "...";
        line = line.Replace("\r", "").Replace("\n", " ").Trim();
        if (line.Length >= 2 && line[0] == '"' && line[^1] == '"')
            line = line.Substring(1, line.Length - 2).Trim();
        if (line.Length > 220)
            line = line.Substring(0, 220).TrimEnd() + "...";
        return line;
    }

    private static string SanitizeOption(string line)
    {
        line = line.Replace("\r", " ").Replace("\n", " ").Trim();
        if (line.Length > 60)
            line = line.Substring(0, 60).TrimEnd() + "...";
        return line;
    }

    private static string DefaultOption(int i) => i switch
    {
        0 => "Hey!",
        1 => "How's your day going?",
        _ => "See you later."
    };
}