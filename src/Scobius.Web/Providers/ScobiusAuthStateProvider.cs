using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Scobius.Web.Providers;

public class ScobiusAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _protectedLocalStorage;
    private string? _token;
    private bool _isInitialized;

    // Serialize concurrent InitializeAsync calls
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public bool IsInitialized => _isInitialized;
    public event Action? Initialized;

    public ScobiusAuthStateProvider(ProtectedLocalStorage protectedLocalStorage)
    {
        _protectedLocalStorage = protectedLocalStorage;
    }

    public async Task InitializeAsync()
    {
        // Fast path — no lock needed if already initialized
        if (_isInitialized)
            return;

        await _initLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_isInitialized)
                return;

            var result = await _protectedLocalStorage.GetAsync<string>("scobius_auth_token");

            if (result.Success && !string.IsNullOrWhiteSpace(result.Value))
            {
                // Validate token before trusting it
                if (!IsTokenExpired(result.Value))
                    _token = result.Value;
                else
                    await _protectedLocalStorage.DeleteAsync("scobius_auth_token");
            }
        }
        catch
        {
            // ProtectedLocalStorage unavailable during SSR — stay anonymous
        }
        finally
        {
            _isInitialized = true;
            _initLock.Release();
        }

        // Notify outside the lock to avoid deadlocks if listeners call back in
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        Initialized?.Invoke();
    }

    public async Task SetTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));

        // Persist first — if storage throws, don't update in-memory state
        await _protectedLocalStorage.SetAsync("scobius_auth_token", token);

        _token = token;
        _isInitialized = true;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task ClearTokenAsync()
    {
        _token = null;
        _isInitialized = true;

        // Notify immediately so UI reflects logout without waiting for storage
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        try
        {
            await _protectedLocalStorage.DeleteAsync("scobius_auth_token");
        }
        catch
        {
            // Best-effort: in-memory is already cleared, UI is already updated
        }
    }

    public string? GetToken() => _token;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (string.IsNullOrEmpty(_token))
            return Anonymous();

        try
        {
            if (IsTokenExpired(_token))
            {
                // Kick off cleanup without blocking the auth state return
                _ = ClearTokenAsync();
                return Anonymous();
            }

            var claims = ParseClaimsFromJwt(_token);
            var identity = new ClaimsIdentity(claims, "jwt");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }
        catch
        {
            _ = ClearTokenAsync();
            return Anonymous();
        }
    }

    private static bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            // Add a 30-second clock skew tolerance
            return jwt.ValidTo < DateTime.UtcNow.AddSeconds(30);
        }
        catch
        {
            return true; // Malformed token → treat as expired
        }
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims;
    }

    private static Task<AuthenticationState> Anonymous() =>
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
}
