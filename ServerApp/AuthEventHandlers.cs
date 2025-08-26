using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Entities;

public static class AuthEventHandlers
{
    public static async Task AuditLoginAsync(TokenValidatedContext context)
    {
        var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

        var email = context.Principal?.FindFirst("email")?.Value
                    ?? context.Principal?.Identity?.Name
                    ?? throw new InvalidOperationException("Email claim not found.");

        // check if user exists
        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            // create default role if needed
            var defaultRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "BasicUser");
            if (defaultRole == null)
            {
                defaultRole = new Roles { Id = Guid.NewGuid(), Name = "BasicUser" };
                db.Roles.Add(defaultRole);
                await db.SaveChangesAsync();
            }

            user = new User
            {
                Id = Guid.NewGuid(),
                ExternalId = context.Principal!.FindFirst("sub")!.Value,
                Email = email,
                RoleId = defaultRole.Id
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        // add audit record
        var audit = new SecurityEvents
        {
            Id = Guid.NewGuid(),
            OccurredUtc = DateTime.UtcNow,
            EventType = "LoginSuccess",
            AuthorUserId = user.Id,
            AffectedUserId = user.Id
        };

        db.SecurityEvents.Add(audit);
        await db.SaveChangesAsync();
    }
    public static async Task AuditLogoutAsync(RedirectContext context)
    {
        var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

        var email = context.HttpContext.User?.FindFirst("email")?.Value 
                    ?? context.HttpContext.User?.Identity?.Name;

        if (string.IsNullOrEmpty(email))
        {
            return; // nothing to log
        }

        var user = db.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            return; // user not tracked locally
        }

        var audit = new SecurityEvents
        {
            Id = Guid.NewGuid(),
            OccurredUtc = DateTime.UtcNow,
            EventType = "LogoutSuccess",
            AuthorUserId = user.Id,
            AffectedUserId = user.Id
        };

        db.SecurityEvents.Add(audit);
        await db.SaveChangesAsync();
    }
}
