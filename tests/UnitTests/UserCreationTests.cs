using System;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Data.Entities;
using GraphQL.Repositories;
using GraphQL.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests
{
    public class UserCreationTests
    {
        private AppDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName) // unique DB per test
                .Options;

            var db = new AppDbContext(options);
            return db;
        }

        [Fact]
        public async Task FirstUser_IsSecurityAuditorWithClaims()
        {
            // Arrange
            using var db = GetDbContext(nameof(FirstUser_IsSecurityAuditorWithClaims));
            var service = new AuditRepository(db); // your class containing CreateUserAsync

            // Act
            var user = await service.CreateUserAsync("first@test.com", "external-1");

            // Assert
            Assert.Equal("SecurityAuditor", user.Role.Name);

            var roleClaims = await db.RoleClaims
                .Include(rc => rc.Claim)
                .Where(rc => rc.RoleId == user.RoleId)
                .ToListAsync();

            Assert.Contains(roleClaims, rc => rc.Claim.Value == "Audit.ViewAuthEvents");
            Assert.Contains(roleClaims, rc => rc.Claim.Value == "Audit.RoleChanges");
        }

        [Fact]
        public async Task SecondUser_IsBasicUserWithNoClaims()
        {
            // Arrange
            using var db = GetDbContext(nameof(SecondUser_IsBasicUserWithNoClaims));
            var service = new AuditRepository(db); // your class containing CreateUserAsync

            // First user to occupy SecurityAuditor role
            await service.CreateUserAsync("first@test.com", "external-1");

            // Act
            var secondUser = await service.CreateUserAsync("second@test.com", "external-2");

            // Assert
            Assert.Equal("BasicUser", secondUser.Role.Name);

            var roleClaims = await db.RoleClaims
                .Include(rc => rc.Claim)
                .Where(rc => rc.RoleId == secondUser.RoleId)
                .ToListAsync();

            Assert.Empty(roleClaims); // BasicUser has no claims
        }
    }
}
