namespace OctocatSupply.Web.Services;

public class AuthService
{
    public bool IsLoggedIn { get; private set; }
    public bool IsAdmin { get; private set; }
    public event Action? OnChange;

    public Task Login(string email, string password)
    {
        // In a real app, you would validate credentials with an API
        // For now, we'll just check the email domain
        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            IsLoggedIn = true;
            IsAdmin = email.EndsWith("@github.com"); // INTENTIONAL BUG: client-side auth spoofing
        }
        OnChange?.Invoke();
        return Task.CompletedTask;
    }

    public void Logout()
    {
        IsLoggedIn = false;
        IsAdmin = false;
        OnChange?.Invoke();
    }
}
