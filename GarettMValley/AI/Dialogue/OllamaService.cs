using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GarettMValley.AI;

/// <summary>
/// Minimal, SMAPI-safe Ollama client for net6.0.
/// Talks to Ollama's REST API directly (no OllamaSharp / Microsoft.Extensions.AI).
/// Default endpoint: http://localhost:11434
/// </summary>
internal sealed class OllamaClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly Uri _baseUri;

    // Safe defaults for SMAPI mods:
    // - Avoid infinite hangs
    // - Keep JSON small and predictable
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OllamaClient(
        string baseUrl = "http://localhost:11434",
        TimeSpan? timeout = null,
        HttpMessageHandler? handler = null)
    {
        _baseUri = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/", UriKind.Absolute);

        _http = handler is null ? new HttpClient() : new HttpClient(handler);
        _http.Timeout = timeout ?? TimeSpan.FromSeconds(30); // IMPORTANT: don't hang the game
        _http.BaseAddress = _baseUri;

        _http.DefaultRequestHeaders.ConnectionClose = false;
    }

    public void Dispose() => _http.Dispose();

    /// <summary>
    /// Simple, non-streaming generate call.
    /// POST /api/generate
    /// Returns the generated text (response field).
    /// </summary>
    public async Task<OllamaGenerateResult> GenerateAsync(
        string model,
        string prompt,
        string? system = null,
        IDictionary<string, object>? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model is required.", nameof(model));
        if (prompt is null)
            throw new ArgumentNullException(nameof(prompt));

        var req = new OllamaGenerateRequest
        {
            Model = model,
            Prompt = prompt,
            System = system,
            Stream = false,
            Options = options
        };

        using var msg = new HttpRequestMessage(HttpMethod.Post, "api/generate")
        {
            Content = new StringContent(JsonSerializer.Serialize(req, JsonOpts), Encoding.UTF8, "application/json")
        };

        using var resp = await _http.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                                    .ConfigureAwait(false);

        var body = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!resp.IsSuccessStatusCode)
        {
            // Ollama often returns {"error":"..."} on failure.
            string details = TryParseError(body) ?? body;
            throw new OllamaHttpException((int)resp.StatusCode, resp.ReasonPhrase ?? "HTTP error", details);
        }

        var parsed = JsonSerializer.Deserialize<OllamaGenerateResponse>(body, JsonOpts)
                        ?? throw new InvalidOperationException("Ollama returned empty JSON.");

        return new OllamaGenerateResult(
            Text: parsed.Response ?? string.Empty,
            Model: parsed.Model,
            CreatedAt: parsed.CreatedAt,
            Done: parsed.Done,
            TotalDuration: parsed.TotalDuration,
            PromptEvalCount: parsed.PromptEvalCount,
            EvalCount: parsed.EvalCount
        );
    }

    /// <summary>
    /// Quick health/ping: GET /
    /// Ollama returns a simple string like "Ollama is running".
    /// </summary>
    public async Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var resp = await _http.GetAsync("", cancellationToken).ConfigureAwait(false);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string? TryParseError(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.String)
                return err.GetString();
        }
        catch { /* ignore */ }
        return null;
    }

    // --- DTOs ---

    private sealed class OllamaGenerateRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "";

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = "";

        [JsonPropertyName("system")]
        public string? System { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        // Options example:
        // { "temperature": 0.7, "top_p": 0.9, "num_predict": 120 }
        [JsonPropertyName("options")]
        public IDictionary<string, object>? Options { get; set; }
    }

    private sealed class OllamaGenerateResponse
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonPropertyName("response")]
        public string? Response { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }

        // These fields exist depending on model / server build; optional.
        [JsonPropertyName("total_duration")]
        public long? TotalDuration { get; set; }

        [JsonPropertyName("prompt_eval_count")]
        public int? PromptEvalCount { get; set; }

        [JsonPropertyName("eval_count")]
        public int? EvalCount { get; set; }
    }
}

internal sealed class OllamaHttpException : Exception
{
    public int StatusCode { get; }
    public string Reason { get; }
    public string Details { get; }

    public OllamaHttpException(int statusCode, string reason, string details)
        : base($"Ollama HTTP {statusCode} ({reason}): {details}")
    {
        StatusCode = statusCode;
        Reason = reason;
        Details = details;
    }
}

internal readonly record struct OllamaGenerateResult(
    string Text,
    string? Model,
    DateTimeOffset? CreatedAt,
    bool Done,
    long? TotalDuration,
    int? PromptEvalCount,
    int? EvalCount
);