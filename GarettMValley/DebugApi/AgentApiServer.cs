using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GarettMValley.AI;
using StardewModdingAPI;

namespace GarettMValley.DebugApi;

/// <summary>
/// Very small localhost HTTP server used for debugging agent internals.
/// 
/// Endpoints:
///   GET /health
///   GET /snapshot           (world + all agents)
///   GET /agents             (list of agents)
///   GET /agents/{id}        (single agent)
/// 
/// If ModConfig.HttpApiKey is non-empty, requests must include header: X-Api-Key
/// </summary>
public sealed class AgentApiServer : IDisposable
{
    private readonly IMonitor _log;
    private readonly AiManager _ai;
    private readonly ModConfig _config;
    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _loopTask;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public AgentApiServer(IMonitor log, AiManager ai, ModConfig config)
    {
        _log = log;
        _ai = ai;
        _config = config;
    }

    public bool IsRunning => _listener.IsListening;

    public void Start()
    {
        if (!_config.EnableHttpApi)
            return;

        if (IsRunning)
            return;

        string bind = string.IsNullOrWhiteSpace(_config.HttpApiBindAddress) ? "127.0.0.1" : _config.HttpApiBindAddress.Trim();
        int port = _config.HttpApiPort <= 0 ? 18080 : _config.HttpApiPort;
        string prefix = $"http://{bind}:{port}/";

        try
        {
            _listener.Prefixes.Clear();
            _listener.Prefixes.Add(prefix);
            _listener.Start();
            _loopTask = Task.Run(() => LoopAsync(_cts.Token));
            _log.Log($"Agent debug HTTP API listening on {prefix}", LogLevel.Info);
        }
        catch (HttpListenerException ex)
        {
            _log.Log($"Failed to start debug HTTP API on {prefix}: {ex.Message}", LogLevel.Error);
            _log.Log("Tip: if the port is in use, change HttpApiPort in config.json.", LogLevel.Info);
        }
        catch (Exception ex)
        {
            _log.Log($"Failed to start debug HTTP API: {ex}", LogLevel.Error);
        }
    }

    public void Stop()
    {
        try { _cts.Cancel(); } catch { }
        try { if (_listener.IsListening) _listener.Stop(); } catch { }
    }

    public void Dispose()
    {
        Stop();
        try { _listener.Close(); } catch { }
        try { _cts.Dispose(); } catch { }
    }

    private async Task LoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var ctx = await _listener.GetContextAsync().ConfigureAwait(false);
                _ = Task.Run(() => HandleRequestAsync(ctx), token);
            }
            catch (ObjectDisposedException) { return; }
            catch (HttpListenerException) { return; } // stopped
            catch (Exception ex)
            {
                _log.Log($"Debug HTTP API loop error: {ex.Message}", LogLevel.Warn);
                await Task.Delay(100, token).ConfigureAwait(false);
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext ctx)
    {
        var req = ctx.Request;
        var res = ctx.Response;

        try
        {
            // CORS preflight (handy if you point a browser UI at this)
            res.Headers["Access-Control-Allow-Origin"] = "*";
            res.Headers["Access-Control-Allow-Methods"] = "GET,OPTIONS";
            res.Headers["Access-Control-Allow-Headers"] = "Content-Type,X-Api-Key";

            if (req.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                res.StatusCode = 204;
                res.Close();
                return;
            }

            if (!req.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                await WriteJsonAsync(res, 405, new { error = "method_not_allowed" });
                return;
            }

            if (!IsAuthorized(req))
            {
                await WriteJsonAsync(res, 401, new { error = "unauthorized" });
                return;
            }

            string path = (req.Url?.AbsolutePath ?? "/").TrimEnd('/');
            if (string.IsNullOrEmpty(path))
                path = "/";

            if (path == "/" || path == "/health")
            {
                await WriteJsonAsync(res, 200, new
                {
                    ok = true,
                    utc = DateTime.UtcNow,
                    worldReady = StardewModdingAPI.Context.IsWorldReady,
                });
                return;
            }

            if (path == "/snapshot")
            {
                await WriteJsonAsync(res, 200, _ai.GetLatestSnapshot());
                return;
            }

            if (path == "/agents")
            {
                var snap = _ai.GetLatestSnapshot();
                await WriteJsonAsync(res, 200, new
                {
                    utc = snap.Utc,
                    worldReady = snap.WorldReady,
                    location = snap.Location,
                    agents = snap.Agents
                });
                return;
            }

            if (path.StartsWith("/agents/", StringComparison.OrdinalIgnoreCase))
            {
                var id = path.Substring("/agents/".Length);
                if (string.IsNullOrWhiteSpace(id))
                {
                    await WriteJsonAsync(res, 400, new { error = "missing_id" });
                    return;
                }

                var agent = _ai.TryGetAgentSnapshot(id);
                if (agent is null)
                {
                    await WriteJsonAsync(res, 404, new { error = "not_found", id });
                    return;
                }

                await WriteJsonAsync(res, 200, agent);
                return;
            }

            await WriteJsonAsync(res, 404, new { error = "not_found" });
        }
        catch (Exception ex)
        {
            try { await WriteJsonAsync(res, 500, new { error = "server_error", message = ex.Message }); }
            catch { }
        }
        finally
        {
            try { res.OutputStream.Close(); } catch { }
            try { res.Close(); } catch { }
        }
    }

    private bool IsAuthorized(HttpListenerRequest req)
    {
        if (string.IsNullOrWhiteSpace(_config.HttpApiKey))
            return true;

        var provided = req.Headers["X-Api-Key"];
        return string.Equals(provided, _config.HttpApiKey, StringComparison.Ordinal);
    }

    private static async Task WriteJsonAsync(HttpListenerResponse res, int status, object payload)
    {
        res.StatusCode = status;
        res.ContentType = "application/json; charset=utf-8";
        await JsonSerializer.SerializeAsync(res.OutputStream, payload, payload.GetType(), JsonOptions)
            .ConfigureAwait(false);
    }
}
