namespace GraphQL.Services
{
    public interface IAuditService
    {
        Task AuditLoginAsync(string? email, string externalId, string provider);
        Task AuditLogoutAsync(string? email, string externalId);
    }
}