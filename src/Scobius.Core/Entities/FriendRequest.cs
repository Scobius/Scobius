namespace Scobius.Core.Entities;

public class FriendRequest
{
    public Guid Id { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public AppUser Sender { get; set; } = null!;
    public AppUser Receiver { get; set; } = null!;
}
