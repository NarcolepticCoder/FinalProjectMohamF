using Data;
using Data.Entities;
using GraphQL;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;


namespace UnitTests
{
    public class QueryResolverUnitTests
    {
        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            // Arrange: unique DB per test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"{nameof(QueryResolverUnitTests)}.{nameof(GetUsers_ReturnsAllUsers)}")
                .Options;

            using var db = new AppDbContext(options);
            var query = new Query();

            var role = new Roles { Id = Guid.NewGuid(), Name = "Admin" };
            db.AddRange(new User { Id = Guid.NewGuid(), ExternalId = "123", Email = "test123@email.com", Role = role },
                        new User { Id = Guid.NewGuid(), ExternalId = "321", Email = "test321@email.com", Role = role });


            db.Roles.Add(role);
            await db.SaveChangesAsync();
            // Act
            var users = query.GetUsers(db).ToList();

            // Assert
            Assert.Equal(2, users.Count);                 // exactly 2 users
            Assert.Contains(users, u => u.Email == "test123@email.com");
            Assert.Contains(users, u => u.Email == "test321@email.com");
            Assert.Contains(users, u => u.ExternalId == "123");
            Assert.Contains(users, u => u.ExternalId == "321");
        
        
            
        }
        [Fact]
        public async Task GetRoles_ReturnsAllRoles()
        {
            // Arrange: unique DB per test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"{nameof(QueryResolverUnitTests)}.{nameof(GetRoles_ReturnsAllRoles)}")
                .Options;

            using var db = new AppDbContext(options);
            var query = new Query();

            db.Roles.AddRange(
                new Roles { Id = Guid.NewGuid(), Name = "Admin" },
                new Roles { Id = Guid.NewGuid(), Name = "User" }
            );
            await db.SaveChangesAsync();

            // Act
            var roles = query.GetRoles(db).ToList();

            // Assert
            Assert.Equal(2, roles.Count);
            Assert.Contains(roles, r => r.Name == "Admin");
            Assert.Contains(roles, r => r.Name == "User");
        }

        [Fact]
        public async Task GetSecurityEvents_ReturnsOrderedEvents()
        {
            // Arrange: unique DB per test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"{nameof(QueryResolverUnitTests)}.{nameof(GetSecurityEvents_ReturnsOrderedEvents)}")
                .Options;

            using var db = new AppDbContext(options);
            var query = new Query();

            var role = new Roles { Id = Guid.NewGuid(), Name = "Admin" };
            var author = new User { Id = Guid.NewGuid(), ExternalId = "123", Email = "author@test.com", Role = role };
            var affected = new User { Id = Guid.NewGuid(), ExternalId = "321", Email = "affected@test.com", Role = role };

            db.Roles.Add(role);
            db.Users.AddRange(author, affected);
            db.SecurityEvents.AddRange(
                new SecurityEvents
                {
                    Id = Guid.NewGuid(),
                    EventType = "Login",
                    AuthorUser = author,
                    AffectedUser = affected,
                    OccurredUtc = DateTime.UtcNow.AddMinutes(-10)
                },
                new SecurityEvents
                {
                    Id = Guid.NewGuid(),
                    EventType = "Logout",
                    AuthorUser = author,
                    AffectedUser = affected,
                    OccurredUtc = DateTime.UtcNow
                }
            );
            await db.SaveChangesAsync();

            // Act
            var events = query.GetSecurityEvents(db).ToList();

            // Assert
            Assert.Equal(2, events.Count);
            Assert.Equal("Logout", events.First().EventType); // newest first
            Assert.Equal("Login", events.Last().EventType);
            Assert.NotNull(events.First().AuthorUser.Role);
            Assert.NotNull(events.First().AffectedUser.Role);
        }
    }
}
