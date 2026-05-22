// Scobius.Infrastructure/Persistence/AppDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    protected override void OnModelCreating(ModelBuilder builder)
        => base.OnModelCreating(builder);
}
