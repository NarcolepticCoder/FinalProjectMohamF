using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.Repositories
{
     public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Users?> GetUserByIdAsync(Guid userId) =>
            await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);

        public async Task<Roles?> GetRoleByIdAsync(Guid roleId) =>
            await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

        public async Task UpdateUserAsync(Users user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task AddSecurityEventAsync(SecurityEvents securityEvent)
        {
            _db.SecurityEvents.Add(securityEvent);
            await _db.SaveChangesAsync();
        }
    }
}