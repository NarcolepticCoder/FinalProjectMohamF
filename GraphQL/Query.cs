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

        [UsePaging]           // optional if you want cursor paging
        [UseFiltering]        // optional if you want filter args
        [UseSorting]          // optional if you want sort args
        public IQueryable<SecurityEvents> GetSecurityEvents(AppDbContext db)
        {


            // Ensure newest first
            return db.SecurityEvents
                     .Include(e => e.AuthorUser)
        .ThenInclude(u => u.Role)
    .Include(e => e.AffectedUser)
        .ThenInclude(u => u.Role)
    .OrderByDescending(e => e.OccurredUtc);
   

                 
        }
        
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