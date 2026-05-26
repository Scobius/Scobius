using Scobius.Core.DTOs.User;

namespace Scobius.Core.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(string userId);
    Task UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task<string> UpdateAvatarAsync(string userId, Stream fileStream, string fileName, string contentType);
    Task<IEnumerable<UserProfileDto>> SearchUsersAsync(string query, string currentUserId);
}