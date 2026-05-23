using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Scobius.Core.Entities;

namespace Scobius.Infrastructure;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<FriendRequest> friendRequests => Set<FriendRequest>();
    public DbSet<Friendship> friendships => Set<Friendship>();


    protected override void OnModelCreating(ModelBuilder builder)
        => base.OnModelCreating(builder);
}
