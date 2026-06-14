namespace Scobius.Web.Models;

public record UserInfoModel
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime? LastSeenAt { get; set; }
}
