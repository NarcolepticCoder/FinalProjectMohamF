using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Users> Users => Set<Users>();
    public DbSet<Roles> Roles => Set<Roles>();
    public DbSet<Claims> Claims => Set<Claims>();
    public DbSet<RoleClaims> RoleClaims => Set<RoleClaims>();
    public DbSet<SecurityEvents> SecurityEvents => Set<SecurityEvents>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique Email
        modelBuilder.Entity<Users>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // User → Role (many-to-one)
        modelBuilder.Entity<Users>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);

        // RoleClaim (many-to-many Role ↔ Claim)
        modelBuilder.Entity<RoleClaims>()
            .HasKey(rc => new { rc.RoleId, rc.ClaimId });

        modelBuilder.Entity<RoleClaims>()
            .HasOne(rc => rc.Role)
            .WithMany(r => r.RoleClaim)
            .HasForeignKey(rc => rc.RoleId);

        modelBuilder.Entity<RoleClaims>()
            .HasOne(rc => rc.Claim)
            .WithMany(c => c.RoleClaim)
            .HasForeignKey(rc => rc.ClaimId);

        // SecurityEvent (self-referencing User FKs)
        modelBuilder.Entity<SecurityEvents>()
            .HasOne(se => se.AuthorUser)
            .WithMany(u => u.AuthoredEvents)
            .HasForeignKey(se => se.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict); // prevent cascade delete loops

        modelBuilder.Entity<SecurityEvents>()
            .HasOne(se => se.AffectedUser)
            .WithMany(u => u.AffectedEvents)
            .HasForeignKey(se => se.AffectedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
