# GMValley (GarettM Valley)
Personal research mod focusing on autonomous agents, and bringing the world of Stardew Valley to life.

> **Status:** Experimental / WIP (0.1.0).  
> Expect bugs, balance issues, and breaking changes while core systems are being built.

---

## Features

### Agent Simulation (WIP)
- Lightweight agent runtime loop (ticks several times/second)
- Emotion model (V/A/D + Stress + needs)
- Sensors + action scoring (utility-style decision)
- Debug overlays for agent state (HUD + labels above NPCs)

### Dialogue Takeover (Optional)
- Harmony patch to intercept NPC talk interactions
- Shows “thinking…” while generating
- Cancels in-flight generations when the dialogue box closes (prevents “ghost replies”)


## Compatibility

### Known compatible
- **Cheat Mod Menu** (disables internal cheat mod config).

### Notes
- This mod uses Harmony patches for dialogue interactions.
- Any mod that heavily replaces NPC talk behavior may conflict, dialogue interactions can be disabled and restricted to emotes for communication.
- If you hit conflicts, try disabling GMValley dialogue takeover first (`AllowDialogue=false`).

---

## Requirements

- **SMAPI** (Stardew Valley mod loader)
- **Ollama** (only if Ollama, and AI dialogue is enabled)
  - Default URL: `http://localhost:11434`
  - Default model: `gemma2:2b`

---

## Installation

1) Install SMAPI.
2) Download/build this mod and place the `GarettMValley/` folder into:
   - `Stardew Valley/Mods/`

3) (Optional) Ollama setup for AI dialogue:
   - Install Ollama
   - Pull the default model: `gemma2:2b`, or pull one of the tested models the mod uses:
     - `ollama pull gemma2:2b`
     - `ollama pull llama3.2:3b`
     - `ollama pull qwen2.5:1.5b`
     - `ollama pull phi3:3.8b`
     - `ollama pull stablelm-zephyr:3b`
   - Start Ollama (default listens on `http://localhost:11434`)

4) Launch Stardew Valley through SMAPI.

---

## Configuration

Config is stored in `config.json` after first launch.

Key options:
- `EnableMod` : master on/off
- `AllowDialogue` : enables dialogue takeover system
- `EnableOllama` : enables AI generation calls
- `OllamaUrl` : Ollama API URL (default `http://localhost:11434`)
- `OllamaModel` : model name (default `gemma2:2b`)
- `OllamaTimeout` : timeout seconds (default `15`)
- `EnableHttpApi` : enables the debug HTTP server
- `HttpApiBindAddress` : default `127.0.0.1`
- `HttpApiPort` : default `18080`
- `HttpApiKey` : optional API key (empty disables auth)

---

## Default Keybinds (Dev / Debug)

- **F8**: Generate AI personalities from game data
- **F9**: Toggle AI debug overlay
- **F10**: Clear active agent runtime cache
- **F12**: Toggle dialogue takeover on/off

(These may change while the mod is still experimental.)

---

## Troubleshooting

### “Dialogue is slow / NPC walked away before response”
- This is expected with slow models.
- Try a smaller/faster model in `OllamaModel`.
- The mod cancels generation when the dialogue box closes, but if you want zero delay, disable AI dialogue.

### “Ollama connection failed”
- Verify Ollama is running and reachable at `OllamaUrl`.
- Check firewall rules if you changed bind address.
- Confirm the model exists: `ollama list`

### “HTTP API won’t start”
- Port may already be in use.
- Change `HttpApiPort` in `config.json`.

---

## Roadmap (Short-term)
- Farm animal adapter + LOD simulation tick
- Persistence (memory across days)
- More sensors/actions grounded in vanilla schedules & locations
- Improved conflict handling with other dialogue mods

---

## License
See `LICENSE`.

---

## Credits
- Garett McCarty (author)
- Stardew Valley / ConcernedApe
- SMAPI community