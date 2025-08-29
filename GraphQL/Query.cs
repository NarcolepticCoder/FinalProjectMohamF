namespace GraphQL
{
    using Data;
    using Data.DTOs;
    using Data.Entities;
    using GraphQL.Services;
    using HotChocolate;
    using HotChocolate.Authorization;
    using Microsoft.EntityFrameworkCore;

    public class Query

    {
        
        
        public IQueryable<User> GetUsers([Service] AppDbContext db) =>
            db.Users.Include(u => u.Role);

        public IQueryable<Roles> GetRoles([Service] AppDbContext db) =>
            db.Roles;

        [UsePaging]
        [UseFiltering]
        [UseSorting]
        public IQueryable<SecurityEvents> GetSecurityEvents([Service] AppDbContext db) =>
            db.SecurityEvents.OrderByDescending(e => e.OccurredUtc);

        
        public async Task<List<ClaimDto>> GetUserClaims(string externalId, [Service] UserService userService)
        {
            if (string.IsNullOrEmpty(externalId))
                return new List<ClaimDto>();

            // Fetch claims from service
            var claims = await userService.GetUserClaimsAsync(externalId);

            return claims;
        }

    }
    
}