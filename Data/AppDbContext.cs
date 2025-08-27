using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Roles> Roles => Set<Roles>();
    public DbSet<Claims> Claims => Set<Claims>();
    public DbSet<RoleClaims> RoleClaims => Set<RoleClaims>();
    public DbSet<SecurityEvents> SecurityEvents => Set<SecurityEvents>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique Email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
            
        var basicUserRoleId = Guid.NewGuid();
        var authObserverRoleId = Guid.NewGuid();
        var securityAuditorRoleId = Guid.NewGuid();

        modelBuilder.Entity<Roles>().HasData(
            new Roles { Id = basicUserRoleId, Name = "BasicUser", Description = "Default role for new users" },
            new Roles { Id = authObserverRoleId, Name = "AuthObserver", Description = "Can view authentication events" },
            new Roles { Id = securityAuditorRoleId, Name = "SecurityAuditor", Description = "Can view auth events & role changes" }
        );

        // --- Seed Claims ---
        var viewAuthEventsClaimId = Guid.NewGuid();
        var roleChangesClaimId = Guid.NewGuid();

        modelBuilder.Entity<Claims>().HasData(
            new Claims { Id = viewAuthEventsClaimId, Type = "permissions", Value = "Audit.ViewAuthEvents" },
            new Claims { Id = roleChangesClaimId, Type = "permissions", Value = "Audit.RoleChanges" }
        );

        // --- Seed RoleClaims mapping ---
        modelBuilder.Entity<RoleClaims>().HasData(
            // AuthObserver → Audit.ViewAuthEvents
            new RoleClaims { RoleId = authObserverRoleId, ClaimId = viewAuthEventsClaimId },

            // SecurityAuditor → both claims
            new RoleClaims { RoleId = securityAuditorRoleId, ClaimId = viewAuthEventsClaimId },
            new RoleClaims { RoleId = securityAuditorRoleId, ClaimId = roleChangesClaimId }
        );

        // User → Role (many-to-one)
        modelBuilder.Entity<User>()
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
