using Scobius.Core.DTOs.User;

namespace Scobius.Core.DTOs.Friendship;

public class FriendRequestDto
{
    public Guid Id { get; set; }
    public UserDto Sender { get; set; } = null!;
    public UserDto Receiver { get; set; } = null!;
    public DateTime SentAt { get; set; }
}