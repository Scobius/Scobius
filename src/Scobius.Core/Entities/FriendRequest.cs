namespace Scobius.Core.Entities;

public class FriendRequest : BaseEntity
{
    public required string SenderId { get; set; }
    public required string ReceiverId { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public AppUser Sender { get; set; } = null!;
    public AppUser Receiver { get; set; } = null!;
}
