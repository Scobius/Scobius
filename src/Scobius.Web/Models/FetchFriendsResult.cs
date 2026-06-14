namespace Scobius.Web.Models;

public record FetchFriendsResult : RequestResult
{
    public UserInfoModel[] Friends { get; set; } = [];
}
