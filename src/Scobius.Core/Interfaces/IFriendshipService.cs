using Scobius.Core.DTOs.Friendship;
using Scobius.Core.DTOs.User;

namespace Scobius.Core.Interfaces;

public interface IFriendshipService
{
    Task SendFriendRequestAsync(string senderId, string receiverId);
    Task AcceptFriendRequestAsync(string userId, Guid requestId);
    Task DeclineFriendRequestAsync(string userId, Guid requestId);
    Task RemoveFriendAsync(string userId, string friendId);
    Task<IEnumerable<UserDto>> GetFriendsAsync(string userId);
    Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(string userId);
}