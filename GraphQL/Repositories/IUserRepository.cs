using Data.Entities;

namespace GraphQL.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<Roles?> GetRoleByIdAsync(Guid roleId);
        Task UpdateUserAsync(User user);
        Task AddSecurityEventAsync(SecurityEvents securityEvent);

        Task<User?> GetUserWithRoleAndClaimsAsync(string externalId);

    }
}