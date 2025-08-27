using Data.Entities;
using GraphQL.Repositories;

namespace GraphQL.Services
{
    public class AuditService
{
    private readonly IAuditRepository _repo;

    public AuditService(IAuditRepository repo)
    {
        _repo = repo;
    }

    public async Task AuditLoginAsync(string email, string externalId, string provider)
    {
        if (string.IsNullOrEmpty(email)) return;

        var user = await _repo.GetUserByEmailAsync(email)
                   ?? await _repo.CreateUserAsync(email, externalId);

        var audit = new SecurityEvents
        {
            Id = Guid.NewGuid(),
            OccurredUtc = DateTime.UtcNow,
            EventType = "LoginSuccess",
            Details = $"provider={provider}",
            AuthorUserId = user.Id,
            AffectedUserId = user.Id
            
        };

        await _repo.AddAuditEventAsync(audit);
    }

    public async Task AuditLogoutAsync(string email, string externalId)
    {
        if (string.IsNullOrEmpty(email)) return;

        var user = await _repo.GetUserByEmailAsync(email);
        if (user == null) return;

        var audit = new SecurityEvents
        {
            Id = Guid.NewGuid(),
            OccurredUtc = DateTime.UtcNow,
            EventType = "LogoutSuccess",
            Details = "local sign out",
            AuthorUserId = user.Id,
            AffectedUserId = user.Id
        };

        await _repo.AddAuditEventAsync(audit);
    }
}
}