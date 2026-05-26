namespace Scobius.Core.Entities;

public class Friendship : BaseEntity
{
    public required string User1Id { get; set; }
    public required string User2Id { get; set; }

    public AppUser User1 { get; set; } = null!;
    public AppUser User2 { get; set; } = null!;
}
