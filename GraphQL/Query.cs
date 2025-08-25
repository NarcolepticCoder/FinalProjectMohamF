namespace GraphQL
{
    using Data;
    using Data.Entities;
    using HotChocolate;
using Microsoft.EntityFrameworkCore;

public class Query
{
    public IQueryable<Users> GetUsers([Service] AppDbContext db) =>
        db.Users.Include(u => u.Role);

    public IQueryable<Roles> GetRoles([Service] AppDbContext db) =>
        db.Roles;

    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<SecurityEvents> GetSecurityEvents([Service] AppDbContext db) =>
        db.SecurityEvents.OrderByDescending(e => e.OccurredUtc);
}
}