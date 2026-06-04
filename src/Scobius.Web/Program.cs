using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Scobius.Web.Components;
using Scobius.Web.Providers;
using Scobius.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Add Authentication services
builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<ScobiusAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(p =>
    p.GetRequiredService<ScobiusAuthStateProvider>()
);

// ProtectedLocalStorage is provided by the Blazor Server hosting model for interactive circuits.
// Explicit registration is safe and makes the auth persistence intent clear.
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedLocalStorage>();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5089"; // Must match the running Scobius.API (see launchSettings / appsettings)

// Add an HttpClient to access the API using the user's cookies
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<IWebAuthService, WebAuthService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IWebUserService, WebUserService>(
    (serviceProvider, c) =>
    {
        c.BaseAddress = new Uri(apiBaseUrl);

        // Attempt to pass the cookie from the current HttpContext to the API (secondary, for future same-origin setups)
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var request = httpContextAccessor.HttpContext?.Request;
        if (request != null && request.Cookies.TryGetValue("Scobius-auth", out var token))
        {
            c.DefaultRequestHeaders.Add("Cookie", $"Scobius-auth={token}");
        }
    }
);

// Named client for use by WebAuthController (and future proxies) to call the backend API
builder.Services.AddHttpClient(
    "ApiClient",
    c =>
    {
        c.BaseAddress = new Uri(apiBaseUrl);
    }
);

builder.Services.AddValidation();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Use Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
