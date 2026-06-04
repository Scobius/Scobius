using Scobius.Web.Models;

namespace Scobius.Web.Services;

public interface IWebUserService
{
    Task<UserProfileModel?> GetProfileAsync();
    Task UpdateProfileAsync(string? displayName, string? bio);
    Task<string> UpdateAvatarAsync(MultipartFormDataContent content);
}
