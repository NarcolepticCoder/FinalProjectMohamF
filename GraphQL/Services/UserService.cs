
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

        public async Task AssignUserRoleAsync(Guid affectedUserId, Guid newRoleId, Guid currentUserId)
        {
            var user = await _repo.GetUserByIdAsync(affectedUserId)
                ?? throw new InvalidOperationException("User not found");

            var oldRole = user.Role?.Name ?? "None";

            var newRole = await _repo.GetRoleByIdAsync(newRoleId)
                ?? throw new InvalidOperationException("Role not found");

            if (user.RoleId == newRole.Id)
                return; // no-op, nothing changed

            // update role
            user.RoleId = newRole.Id;
            user.Role = newRole;
            await _repo.UpdateUserAsync(user);

            // emit security event
            var evt = new SecurityEvents
            {
                Id = Guid.NewGuid(),
                AuthorUserId = currentUserId, // who did it
                AffectedUserId = affectedUserId, // who was changed
                EventType = "RoleAssigned",
                Details = $"from={oldRole} to={newRole.Name}",
                OccurredUtc = DateTime.UtcNow
            };

            await _repo.AddSecurityEventAsync(evt);
        }
    }
}
