using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Domain.Identity;
using Selu383.SP26.Api.Features.Locations;


namespace Selu383.SP26.Api.Data;

public class DataContext : IdentityDbContext<
    User, Role, int,
    IdentityUserClaim<int>, UserRole,
    IdentityUserLogin<int>,
    IdentityRoleClaim<int>,
    IdentityUserToken<int>>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Location> Locations { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.Entity<UserRole>(ur =>
    {
        ur.HasKey(x => new { x.UserId, x.RoleId });

        ur.HasOne(x => x.User)
            .WithMany(u => u.Roles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        ur.HasOne(x => x.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    });
}

}
