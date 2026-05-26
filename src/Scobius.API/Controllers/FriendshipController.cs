using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scobius.Core.Interfaces;
using System.Security.Claims;

namespace Scobius.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FriendshipController : ControllerBase
{
    private readonly IFriendshipService _friendshipService;

    public FriendshipController(IFriendshipService friendshipService)
    {
        _friendshipService = friendshipService;
    }

    [HttpPost("request/{receiverId}")]
    public async Task<IActionResult> SendRequest(string receiverId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _friendshipService.SendFriendRequestAsync(userId, receiverId);
            return Ok(new { message = "Friend request sent." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("request/{requestId}/accept")]
    public async Task<IActionResult> AcceptRequest(Guid requestId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _friendshipService.AcceptFriendRequestAsync(userId, requestId);
            return Ok(new { message = "Friend request accepted." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("request/{requestId}/decline")]
    public async Task<IActionResult> DeclineRequest(Guid requestId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _friendshipService.DeclineFriendRequestAsync(userId, requestId);
            return Ok(new { message = "Friend request declined." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{friendId}")]
    public async Task<IActionResult> RemoveFriend(string friendId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _friendshipService.RemoveFriendAsync(userId, friendId);
            return Ok(new { message = "Friend removed." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var friends = await _friendshipService.GetFriendsAsync(userId);
            return Ok(friends);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("requests/pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var requests = await _friendshipService.GetPendingRequestsAsync(userId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}