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

    public async Task<User> GetUserByEmailAsync(string email)
    {
        return await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateUserAsync(string email, string externalId)
    {
        var defaultRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "BasicUser");
        if (defaultRole == null)
        {
            defaultRole = new Roles { Id = Guid.NewGuid(), Name = "BasicUser" };
            _db.Roles.Add(defaultRole);
            await _db.SaveChangesAsync();
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            ExternalId = externalId,
            RoleId = defaultRole.Id
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