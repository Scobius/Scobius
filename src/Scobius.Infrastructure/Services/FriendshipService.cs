using Microsoft.EntityFrameworkCore;
using Scobius.Core.DTOs.Friendship;
using Scobius.Core.DTOs.User;
using Scobius.Core.Entities;
using Scobius.Core.Interfaces;

namespace Scobius.Infrastructure.Services;

public class FriendshipService : IFriendshipService
{
    private readonly AppDbContext _db;

    public FriendshipService(AppDbContext db)
    {
        _db = db;
    }

    public async Task SendFriendRequestAsync(string senderId, string receiverId)
    {
        if (senderId == receiverId)
            throw new Exception("You cannot send a friend request to yourself.");

        var existingRequest = await _db.friendRequests.AnyAsync(r =>
            (r.SenderId == senderId && r.ReceiverId == receiverId) ||
            (r.SenderId == receiverId && r.ReceiverId == senderId));

        if (existingRequest)
            throw new Exception("A friend request already exists between these users.");

        var alreadyFriends = await _db.friendships.AnyAsync(f =>
            (f.User1Id == senderId && f.User2Id == receiverId) ||
            (f.User1Id == receiverId && f.User2Id == senderId));

        if (alreadyFriends)
            throw new Exception("You are already friends with this user.");

        var request = new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId
        };

        _db.friendRequests.Add(request);
        await _db.SaveChangesAsync();
    }

    public async Task AcceptFriendRequestAsync(string userId, Guid requestId)
    {
        var request = await _db.friendRequests.FindAsync(requestId)
            ?? throw new Exception("Friend request not found.");

        if (request.ReceiverId != userId)
            throw new Exception("You can only accept requests sent to you.");

        var user1Id = request.SenderId;
        var user2Id = request.ReceiverId;

        // Ensure User1Id < User2Id for consistency
        if (string.Compare(user1Id, user2Id) > 0)
        {
            (user1Id, user2Id) = (user2Id, user1Id);
        }

        var friendship = new Friendship
        {
            User1Id = user1Id,
            User2Id = user2Id
        };

        _db.friendships.Add(friendship);
        _db.friendRequests.Remove(request);
        await _db.SaveChangesAsync();
    }

    public async Task DeclineFriendRequestAsync(string userId, Guid requestId)
    {
        var request = await _db.friendRequests.FindAsync(requestId)
            ?? throw new Exception("Friend request not found.");

        if (request.ReceiverId != userId && request.SenderId != userId)
            throw new Exception("You are not involved in this request.");

        _db.friendRequests.Remove(request);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveFriendAsync(string userId, string friendId)
    {
        var friendship = await _db.friendships.FirstOrDefaultAsync(f =>
            (f.User1Id == userId && f.User2Id == friendId) ||
            (f.User1Id == friendId && f.User2Id == userId))
            ?? throw new Exception("Friendship not found.");

        _db.friendships.Remove(friendship);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserDto>> GetFriendsAsync(string userId)
    {
        var friendships = await _db.friendships
            .Include(f => f.User1)
            .Include(f => f.User2)
            .Where(f => f.User1Id == userId || f.User2Id == userId)
            .ToListAsync();

        return friendships.Select(f =>
        {
            var friend = f.User1Id == userId ? f.User2 : f.User1;
            return MapToUserDto(friend);
        });
    }

    public async Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(string userId)
    {
        var requests = await _db.friendRequests
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .Where(r => r.ReceiverId == userId)
            .ToListAsync();

        return requests.Select(r => new FriendRequestDto
        {
            Id = r.Id,
            SentAt = r.SentAt,
            Sender = MapToUserDto(r.Sender),
            Receiver = MapToUserDto(r.Receiver)
        });
    }

    private static UserDto MapToUserDto(AppUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            LastSeenAt = user.LastSeenAt
        };
    }
}