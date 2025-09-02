
using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using GraphQL.Repositories;

namespace GraphQL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

    public async Task<AssignRoleResult> AssignUserRoleAsync(
        Guid affectedUserId, Guid newRoleId, Guid currentUserId)
    {
            // Get users
            var affectedUser = await _repo.GetUserByIdAsync(affectedUserId)
                ?? throw new InvalidOperationException("Target user not found");

            var authorUser = await _repo.GetUserByIdAsync(currentUserId)
                ?? throw new InvalidOperationException("Current user not found");

            // Save old role name
            var oldRole = affectedUser.Role?.Name ?? "None";

            // Get new role
            var newRole = await _repo.GetRoleByIdAsync(newRoleId)
                ?? throw new InvalidOperationException("Role not found");

            // Update role if different
            if (affectedUser.RoleId != newRole.Id)
            {
                affectedUser.RoleId = newRole.Id;
                affectedUser.Role = newRole;
                await _repo.UpdateUserAsync(affectedUser);
            }

            // Add security event
            var evt = new SecurityEvents
            {
                AuthorUserId = currentUserId,
                AffectedUserId = affectedUserId,
                EventType = "RoleAssigned",
                Details = $"from={oldRole} to={newRole.Name}",
                OccurredUtc = DateTime.UtcNow
            };
            await _repo.AddSecurityEventAsync(evt);

            // Return full users
            return new AssignRoleResult
            {
                AuthorUser = authorUser,
                AffectedUser = affectedUser,
                FromRole = oldRole,
                ToRole = newRole.Name
            };
        }



        public async Task<List<ClaimDto>> GetUserClaimsAsync(string? externalId)
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
