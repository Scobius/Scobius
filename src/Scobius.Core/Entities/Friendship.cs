namespace Scobius.Core.Entities;

public class Friendship
{
    public Guid Id { get; set; }
    public string User1Id { get; set; }
    public string User2Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
