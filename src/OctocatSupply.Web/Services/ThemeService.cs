namespace OctocatSupply.Web.Services;

public class ThemeService
{
    public bool DarkMode { get; set; } = false;
    public event Action? OnChange;

    public void ToggleTheme()
    {
        DarkMode = !DarkMode;
        OnChange?.Invoke();
    }
}
