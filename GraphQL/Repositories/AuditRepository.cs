using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly AppDbContext _db;

        public AuditRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(string email, string externalId)
        {
            // Determine if this is the first user
            var isFirstUser = !await _db.Users.AnyAsync();

            // Determine the role
            var roleName = isFirstUser ? "SecurityAuditor" : "BasicUser";

            // Get or create the role
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
            {
                role = new Roles { Id = Guid.NewGuid(), Name = roleName };
                _db.Roles.Add(role);
                
            }

            // Ensure the role has the correct claims
            var claimsToAssign = roleName switch
            {
                "SecurityAuditor" => new[] { "Audit.ViewAuthEvents", "Audit.RoleChanges" },
                "AuthObserver" => new[] { "Audit.ViewAuthEvents" },
                _ => Array.Empty<string>()
            };

            foreach (var claimValue in claimsToAssign)
            {
                var claim = await _db.Claims.FirstOrDefaultAsync(c => c.Type == "permissions" && c.Value == claimValue);
                if (claim == null)
                {
                    claim = new Claims { Id = Guid.NewGuid(), Type = "permissions", Value = claimValue };
                    _db.Claims.Add(claim);
                    
                }

                var hasMapping = await _db.RoleClaims.AnyAsync(rc => rc.RoleId == role.Id && rc.ClaimId == claim.Id);
                if (!hasMapping)
                {
                    _db.RoleClaims.Add(new RoleClaims { RoleId = role.Id, ClaimId = claim.Id });
                    
                }
            }

            // Create the user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                ExternalId = externalId,
                RoleId = role.Id
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return user;
        }


        public async Task AddAuditEventAsync(SecurityEvents auditEvent)
        {
            _db.SecurityEvents.Add(auditEvent);
            await _db.SaveChangesAsync();
        }
    }
}