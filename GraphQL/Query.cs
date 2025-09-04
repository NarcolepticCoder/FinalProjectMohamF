using System.Security.Claims;
using Data;
using Data.DTOs;
using Data.Entities;
using GraphQL.Services;
using HotChocolate;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GraphQL
{

    
    public class Query

    {

        [Authorize]
        public IQueryable<User> GetUsers([Service] AppDbContext db) =>
            db.Users.Include(u => u.Role);

        [Authorize]
        public IQueryable<Roles> GetRoles([Service] AppDbContext db) =>
            db.Roles;

        //can use if has ViewAuthEvents or RoleChanges
        [Authorize(Policy = "CanViewAnySecurityEvents")]
        public IQueryable<SecurityEvents> GetSecurityEvents([Service] AppDbContext db, [Service] IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext!.User;

            var canViewAuthEvents = user.HasClaim("permissions", "Audit.ViewAuthEvents");
            var canViewRoleChanges = user.HasClaim("permissions", "Audit.RoleChanges");

            var events = db.SecurityEvents
                .Include(e => e.AuthorUser).ThenInclude(u => u.Role)
                .Include(e => e.AffectedUser).ThenInclude(u => u.Role)
                .OrderByDescending(e => e.OccurredUtc);

            return events.Where(e =>
                (canViewAuthEvents && (e.EventType == "LoginSuccess" || e.EventType == "LogoutSuccess")) ||
                (canViewRoleChanges && e.EventType == "RoleAssigned")
            );
        }




        public async Task<List<ClaimDto>> GetUserClaims(string externalId, [Service] IUserService userService)
        {
            if (string.IsNullOrEmpty(externalId))
                return new List<ClaimDto>();

            // Fetch claims from service
            var claims = await userService.GetUserClaimsAsync(externalId);

            return claims;
        }

    }

}