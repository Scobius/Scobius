using Scobius.Web.Models;

namespace Scobius.Web.Services;

public interface IWebAuthService
{
    Task<AuthResult> LoginAsync(string emailOrUsername, string password);
    Task<string> RegisterAsync(string username, string displayName, string email, string password);
    Task LogoutAsync();
}
