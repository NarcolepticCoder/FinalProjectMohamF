
using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using GraphQL.Repositories;

namespace GraphQL.Services
{
    public class UserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<AssignRoleResult> AssignUserRoleAsync(
    Guid affectedUserId, Guid newRoleId, Guid currentUserId)
        {
            var user = await _repo.GetUserByIdAsync(affectedUserId)
                ?? throw new InvalidOperationException("User not found");

            var oldRole = user.Role?.Name ?? "None";

            var newRole = await _repo.GetRoleByIdAsync(newRoleId)
                ?? throw new InvalidOperationException("Role not found");

            if (user.RoleId == newRole.Id)
            {
                return new AssignRoleResult
                {
                    UserId = user.Id,
                    RoleId = user.RoleId,
                    FromRole = oldRole,
                    ToRole = newRole.Name
                };
            }

            user.RoleId = newRole.Id;
            user.Role = newRole;
            await _repo.UpdateUserAsync(user);

            var evt = new SecurityEvents
            {
                Id = Guid.NewGuid(),
                AuthorUserId = currentUserId,
                AffectedUserId = affectedUserId,
                EventType = "RoleAssigned",
                Details = $"from={oldRole} to={newRole.Name}",
                OccurredUtc = DateTime.UtcNow
            };

            await _repo.AddSecurityEventAsync(evt);

            return new AssignRoleResult
            {
                UserId = user.Id,
                RoleId = newRole.Id,
                FromRole = oldRole,
                ToRole = newRole.Name
            };
        }

        public async Task<List<ClaimDto>> GetUserClaimsAsync(string externalId)
        {
            if (string.IsNullOrEmpty(externalId))
                return new List<ClaimDto>();

            // Fetch user including role and role claims
            var user = await _repo.GetUserWithRoleAndClaimsAsync(externalId);
            if (user == null || user.Role == null)
                return new List<ClaimDto>();

            var claims = new List<ClaimDto>
        {
            // Always include the role itself
            new ClaimDto { Type = ClaimTypes.Role, Value = user.Role.Name }
        };

            // Add all claims associated with that role
            claims.AddRange(user.Role.RoleClaim.Select(rc =>
                new ClaimDto { Type = rc.Claim.Type, Value = rc.Claim.Value }));

            return claims;
        }
    
    }
}
