namespace Scobius.Web.Models;

public class AuthResult
{
    public bool Success { get; set; } = true;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime AccessTokenExpiry { get; set; }
    public string? Error { get; set; }
}
