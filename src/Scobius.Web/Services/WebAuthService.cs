using Scobius.Web.Models;
using Scobius.Web.Providers;

namespace Scobius.Web.Services;

public class WebAuthService(HttpClient http, ScobiusAuthStateProvider authProvider)
    : IWebAuthService
{
    private readonly HttpClient _http = http;
    private readonly ScobiusAuthStateProvider _authProvider = authProvider;

    public async Task<AuthResult> LoginAsync(string emailOrUsername, string password)
    {
        var response = await _http.PostAsJsonAsync(
            "api/auth/login",
            new { EmailOrUsername = emailOrUsername, Password = password }
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return new AuthResult { Success = false, Error = error };
        }

        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        if (result?.AccessToken != null)
            await _authProvider.SetTokenAsync(result.AccessToken);

        return result!;
    }

    public async Task<string> RegisterAsync(
        string username,
        string displayName,
        string email,
        string password
    )
    {
        var response = await _http.PostAsJsonAsync(
            "api/auth/register",
            new
            {
                Username = username,
                DisplayName = displayName,
                Email = email,
                Password = password,
            }
        );

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception(content);

        return content;
    }

    public async Task LogoutAsync()
    {
        try
        {
            // Prefer the web surface (clears web cookie + tells backend)
            await _http.PostAsync("api/web-auth/logout", null);
        }
        catch
        {
            // fallback direct to backend
            try
            {
                await _http.PostAsync("api/auth/logout", null);
            }
            catch { }
        }

        await _authProvider.ClearTokenAsync();
    }
}
