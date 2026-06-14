namespace Scobius.Web.Models;

public record AuthResult : RequestResult
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime AccessTokenExpiry { get; set; }
}
