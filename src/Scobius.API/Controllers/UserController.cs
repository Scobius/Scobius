using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scobius.Core.DTOs.User;
using Scobius.Core.Interfaces;
using System.Security.Claims;

namespace Scobius.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfile(string id)
    {
        try
        {
            var profile = await _userService.GetProfileAsync(id);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string q)
    {
        try
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var results = await _userService.SearchUsersAsync(q, currentUserId);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _userService.UpdateProfileAsync(userId, request);
            return Ok(new { message = "Profile updated." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("profile/avatar")]
    public async Task<IActionResult> UpdateAvatar(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            using var stream = file.OpenReadStream();
            var publicUrl = await _userService.UpdateAvatarAsync(userId, stream, file.FileName, file.ContentType);
            
            return Ok(new { avatarUrl = publicUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}