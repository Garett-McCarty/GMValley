using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace GarettMValley.UI;

internal sealed class UiManager
{
    private readonly IMonitor _monitor;
    private readonly IModHelper _helper;

    private readonly Func<ModConfig> _getConfig;
    private readonly Action<ModConfig> _setConfig;

    private readonly Action<ModConfig> _onConfigApplied;

    public bool UseCustomConfigMenu { get; private set; } = true;

    public UiManager(
        IMonitor monitor,
        IModHelper helper,
        Func<ModConfig> getConfig,
        Action<ModConfig> setConfig,
        Action<ModConfig> onConfigApplied
    )
    {
        _monitor = monitor;
        _helper = helper;
        _getConfig = getConfig;
        _setConfig = setConfig;
        _onConfigApplied = onConfigApplied;
    }

    public void Initialize()
    {
        if (_helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
        {
            UseCustomConfigMenu = false;
            _monitor.Log("Generic Mod Config Menu detected. Custom config UI disabled.", LogLevel.Info);
        }

        _helper.Events.Input.ButtonPressed += OnButtonPressed;
    }

    public void Shutdown()
    {
        _helper.Events.Input.ButtonPressed -= OnButtonPressed;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;
        
        if (Game1.activeClickableMenu != null)
            return;
            
        if (Game1.keyboardDispatcher?.Subscriber != null)
            return;

        if (e.Button != SButton.K)
            return;

        TryOpenConfigMenu();
    }

    public bool TryOpenConfigMenu()
    {
        if (!UseCustomConfigMenu)
            return false;

        if (Game1.activeClickableMenu != null)
            return false;

        Game1.activeClickableMenu = new ModConfigMenu(
            _monitor,
            _helper,
            _getConfig,
            cfg =>
            {
                _setConfig(cfg);
            },
            cfg => _onConfigApplied(cfg)
        );

        Game1.playSound("bigSelect");
        return true;
    }
}