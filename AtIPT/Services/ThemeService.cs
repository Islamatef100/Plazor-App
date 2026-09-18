using AtIPT.Application.Abstractions;
using Microsoft.JSInterop;

namespace AtIPT.Services;

public sealed class ThemeService : IThemeService
{
    private const string StorageKey = "atipt-theme";
    private readonly IJSRuntime _js;

    public AppTheme Current { get; private set; } = AppTheme.Light;
    public event Action? OnChange;

    public ThemeService(IJSRuntime js) => _js = js;

    public async Task InitialiseAsync()
    {
        try
        {
            var stored = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (Enum.TryParse<AppTheme>(stored, out var t) && t != Current)
            {
                Current = t;
                OnChange?.Invoke();
            }
        }
        catch
        {
            // Prerender cannot reach JS. Default theme stays Light.
        }
    }

    public async Task SetThemeAsync(AppTheme theme)
    {
        Current = theme;
        OnChange?.Invoke();

        try { await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, theme.ToString()); }
        catch { }
    }

    public Task ToggleAsync() =>
        SetThemeAsync(Current == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
}