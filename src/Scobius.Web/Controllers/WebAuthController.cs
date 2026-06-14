using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Scobius.Web.Models;

namespace Scobius.Web.Controllers;

/// <summary>
/// Web-specific auth surface (BFF-lite). Proxies to the main API and can manage
/// the Scobius-auth cookie for this origin as a secondary mechanism.
/// Primary auth persistence for the Blazor web client uses ProtectedLocalStorage via the state provider.
/// </summary>
[ApiController]
[Route("api/web-auth")]
public class WebAuthController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginPayload payload)
    {
        if (
            string.IsNullOrWhiteSpace(payload.EmailOrUsername)
            || string.IsNullOrWhiteSpace(payload.Password)
        )
            return BadRequest(new { error = "Email/username and password are required." });

        var apiClient = _httpClientFactory.CreateClient("ApiClient");

        var response = await apiClient.PostAsJsonAsync(
            "api/auth/login",
            new { EmailOrUsername = payload.EmailOrUsername, Password = payload.Password }
        );

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            return BadRequest(new { error = err });
        }

        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        if (result?.AccessToken != null)
        {
            // Set cookie for this Web origin (useful for full page loads / future same-site hosting)
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.AccessToken);

            Response.Cookies.Append(
                "Scobius-auth",
                result.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = jwt.ValidTo,
                }
            );
        }

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // Clear our cookie
        Response.Cookies.Delete("Scobius-auth");

        // Best-effort: also tell the backend to clear/revoke its view of the token
        try
        {
            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            // The backend logout currently just deletes its cookie; calling it is harmless.
            await apiClient.PostAsync("api/auth/logout", null);
        }
        catch
        {
            // ignore network errors on logout
        }

        return Ok(new { message = "Logged out." });
    }

    public record LoginPayload(string EmailOrUsername, string Password);
}
