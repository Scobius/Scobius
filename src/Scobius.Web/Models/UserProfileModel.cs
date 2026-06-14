namespace Scobius.Web.Models;

public record UserProfileModel : UserInfoModel
{
    public DateTime CreatedAt { get; set; }
}
