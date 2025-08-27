using Data.Entities;

namespace GraphQL.Repositories
{
    public interface IAuditRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(string email, string externalId);
        Task AddAuditEventAsync(SecurityEvents auditEvent);
    }

}