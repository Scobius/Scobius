using System.Net.Http.Headers;
using System.Net.Http.Json;
using Scobius.Web.Models;
using Scobius.Web.Providers;

namespace Scobius.Web.Services;

public class WebUserService(HttpClient http, ScobiusAuthStateProvider authProvider)
    : IWebUserService
{
    private readonly HttpClient _http = http;
    private readonly ScobiusAuthStateProvider _authProvider = authProvider;

    // Build a fresh request with the auth header instead of mutating DefaultRequestHeaders
    private HttpRequestMessage AuthorizedRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        var token = _authProvider.GetToken();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    public async Task<UserProfileModel?> GetProfileAsync()
    {
        var request = AuthorizedRequest(HttpMethod.Get, "api/user");
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserProfileModel>();
    }

    public async Task UpdateProfileAsync(string? displayName, string? bio)
    {
        var request = AuthorizedRequest(HttpMethod.Put, "api/user/profile");
        request.Content = JsonContent.Create(new { DisplayName = displayName, Bio = bio });
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> UpdateAvatarAsync(MultipartFormDataContent content)
    {
        var request = AuthorizedRequest(HttpMethod.Post, "api/user/profile/avatar");
        request.Content = content;
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AvatarUpdateResult>();
        return result?.AvatarUrl ?? string.Empty;
    }
}

