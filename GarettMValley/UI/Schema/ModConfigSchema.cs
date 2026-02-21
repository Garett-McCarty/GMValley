using System;
using System.Collections.Generic;

namespace GarettMValley.UI.Schema;

internal static class ModConfigSchema
{
    public static List<ConfigField> Build(ModConfig cfg)
    {
        return new()
        {
            new HeaderField("H_General", "General"),

            new BoolField("EnableMod", "Enable Mod", () => cfg.EnableMod, v => cfg.EnableMod = v,
                "Master toggle for the mod."),

            new StringField("Message", "Welcome Message", () => cfg.Message, v => cfg.Message = v),

            new HeaderField("H_Dialogue", "Dialogue"),
            new BoolField("AllowDialogue", "Allow custom dialogue", () => cfg.AllowDialogue, v => cfg.AllowDialogue = v),
            new BoolField("AllowGeneratedDialogue", "Allow generated dialogue", () => cfg.AllowGeneratedDialogue, v => cfg.AllowGeneratedDialogue = v),

            new HeaderField("H_Agency", "Agency"),
            new BoolField("AllowVillagerAgency", "Allow villager agency", () => cfg.AllowVillagerAgency, v => cfg.AllowVillagerAgency = v),
            new BoolField("AllowMonsterAgency", "Allow monster agency", () => cfg.AllowMonsterAgency, v => cfg.AllowMonsterAgency = v),
            new BoolField("AllowPetsAgency", "Allow pets agency", () => cfg.AllowPetsAgency, v => cfg.AllowPetsAgency = v),
            new BoolField("AllowAnimalAgency", "Allow farm animal agency", () => cfg.AllowAnimalAgency, v => cfg.AllowAnimalAgency = v),

            new HeaderField("H_Ollama", "Ollama"),
            new BoolField("EnableOllama", "Enable Ollama", () => cfg.EnableOllama, v => cfg.EnableOllama = v),
            new StringField("OllamaUrl", "Ollama URL",
                () => cfg.OllamaUrl,
                v => cfg.OllamaUrl = v.Trim(),
                validate: s => Uri.TryCreate(s, UriKind.Absolute, out _) ? null : "Must be a valid URL."),

            new ChoiceField(
                id: "OllamaModel",
                label: "Ollama Model",
                get: () => cfg.OllamaModel,
                set: v => cfg.OllamaModel = v,
                options: new[]
                {
                    "gemma2:2b",
                    "llama3.2:3b",
                    "qwen2.5:1.5b",
                    "phi3:3.8b",
                    "stablelm-zephyr:3b"
                },
                tooltip: "Pick the Ollama model tag to use."
            ),

            new IntField("OllamaTimeout", "Ollama Timeout (seconds)", () => cfg.OllamaTimeout, v => cfg.OllamaTimeout = v, 1, 120),

            new HeaderField("H_HttpApi", "HTTP API"),
            new BoolField("EnableHttpApi", "Enable HTTP API", () => cfg.EnableHttpApi, v => cfg.EnableHttpApi = v),
            new StringField("HttpApiBindAddress", "Bind Address", () => cfg.HttpApiBindAddress, v => cfg.HttpApiBindAddress = v.Trim()),
            new IntField("HttpApiPort", "Port", () => cfg.HttpApiPort, v => cfg.HttpApiPort = v, 1024, 65535),
            new StringField("HttpApiKey", "API Key", () => cfg.HttpApiKey, v => cfg.HttpApiKey = v),

            new HeaderField("H_Debug", "Debug"),
            new BoolField("Debug", "Debug Mode", () => cfg.Debug, v => cfg.Debug = v),
            new BoolField("DebugLog", "Debug Logging", () => cfg.DebugLog, v => cfg.DebugLog = v),
        };
    }
}