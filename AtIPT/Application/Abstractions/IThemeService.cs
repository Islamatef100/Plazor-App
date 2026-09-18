namespace AtIPT.Application.Abstractions;

public enum AppTheme { Light, Dark }

public interface IThemeService
{
    AppTheme Current { get; }
    event Action? OnChange;
    Task InitialiseAsync();
    Task SetThemeAsync(AppTheme theme);
    Task ToggleAsync();
}