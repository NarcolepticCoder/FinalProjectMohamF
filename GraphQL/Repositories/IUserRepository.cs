using Data.Entities;

namespace GraphQL.Repositories
{
    public interface IUserRepository
    {
        Task<Users?> GetUserByIdAsync(Guid userId);
        Task<Roles?> GetRoleByIdAsync(Guid roleId);
        Task UpdateUserAsync(Users user);
        Task AddSecurityEventAsync(SecurityEvents securityEvent);
    }
}