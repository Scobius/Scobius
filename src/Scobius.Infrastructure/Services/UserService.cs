using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Scobius.Core.DTOs.User;
using Scobius.Core.Entities;
using Scobius.Core.Interfaces;

namespace Scobius.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IFileService _fileService;
    private readonly IConfiguration _config;

    public UserService(UserManager<AppUser> userManager, IFileService fileService, IConfiguration config)
    {
        _userManager = userManager;
        _fileService = fileService;
        _config = config;
    }

    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found");

        return MapToProfileDto(user);
    }

    public async Task UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found");

        if (request.DisplayName != null)
            user.DisplayName = request.DisplayName;

        if (request.Bio != null)
            user.Bio = request.Bio;

        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<string> UpdateAvatarAsync(string userId, Stream fileStream, string fileName, string contentType)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found");

        var bucket = _config["Supabase:Bucket"] ?? "avatars";
        // Generate a unique filename to avoid collisions and caching issues
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{userId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{extension}";

        var publicUrl = await _fileService.UploadFileAsync(fileStream, uniqueFileName, contentType, bucket);

        user.AvatarUrl = publicUrl;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        return publicUrl;
    }

    public async Task<IEnumerable<UserProfileDto>> SearchUsersAsync(string query, string currentUserId)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<UserProfileDto>();

        var normalizedQuery = query.ToUpper();

        var users = await _userManager.Users
            .Where(u => u.Id != currentUserId)
            .Where(u => u.NormalizedUserName!.Contains(normalizedQuery) || 
                        u.NormalizedEmail!.Contains(normalizedQuery) ||
                        u.DisplayName.ToUpper().Contains(normalizedQuery))
            .Take(20)
            .ToListAsync();

        return users.Select(MapToProfileDto);
    }

    private static UserProfileDto MapToProfileDto(AppUser user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            LastSeenAt = user.LastSeenAt,
            CreatedAt = user.CreatedAt
        };
    }
}