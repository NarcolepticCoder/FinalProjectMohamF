using System.Security.Claims;
using Data;
using Data.Entities;
using FluentAssertions;
using GraphQL.Repositories;
using GraphQL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
namespace UnitTests
{

    public class UnitTest1
    {
        [Fact]
        public async Task UserRoleAssignedShouldWrite_RoleAssignedEventOnSave()
        {
            // --- Arrange ---
            SecurityEvents? capturedEvent = null;

            var authorUserId = Guid.NewGuid();
            var affectedUserId = Guid.NewGuid();

            var oldRole = new Roles { Id = Guid.NewGuid(), Name = "BasicUser" };
            var newRole = new Roles { Id = Guid.NewGuid(), Name = "AuthObserver" };

            var affectedUser = new User
            {
                Id = affectedUserId,
                Email = "jane@example.com",
                Role = oldRole,
                RoleId = oldRole.Id
            };

            // Mock repository or DbContext
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.GetUserByIdAsync(affectedUserId))
                    .ReturnsAsync(affectedUser);
            mockRepo.Setup(r => r.GetRoleByIdAsync(newRole.Id))
                    .ReturnsAsync(newRole);
            mockRepo.Setup(r => r.AddSecurityEventAsync(It.IsAny<SecurityEvents>()))
                    .Callback<SecurityEvents>(e => capturedEvent = e)
                    .Returns(Task.CompletedTask);

            var service = new UserService(mockRepo.Object);

            // --- Act ---
            await service.AssignUserRoleAsync(affectedUserId, newRole.Id, authorUserId);

            // --- Assert ---
            affectedUser.RoleId.Should().Be(newRole.Id);
            affectedUser.Role.Name.Should().Be(newRole.Name);

            capturedEvent.Should().NotBeNull();
            capturedEvent!.AuthorUserId.Should().Be(authorUserId);
            capturedEvent.AffectedUserId.Should().Be(affectedUserId);
            capturedEvent.EventType.Should().Be("RoleAssigned");
            capturedEvent.Details.Should().Be($"from={oldRole.Name} to={newRole.Name}");


            mockRepo.Verify(r => r.AddSecurityEventAsync(It.IsAny<SecurityEvents>()), Times.Once);

        }
        [Fact]
        public async Task AuditLoginAsync_ShouldCreateUserAndAddLoginSuccessEvent()
        {
            // --- Setup in-memory EF DbContext ---
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("AuditLoginTestDb")
                .Options;

            using var db = new AppDbContext(options);

            // --- Setup ClaimsPrincipal ---
            var email = "jane@example.com";
            var externalId = "sub-123";
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim("email", email),
            new Claim("sub", externalId)
            }, "TestAuth"));

            // --- Setup HttpContext with DI ---
            var httpContext = new DefaultHttpContext
            {
                User = principal,
                RequestServices = new ServiceCollection()
                    .AddSingleton(db)
                    .BuildServiceProvider()
            };

            // --- Setup AuthenticationScheme ---
            var scheme = new AuthenticationScheme("TestScheme", "TestScheme", typeof(OpenIdConnectHandler));

            // --- Setup AuthenticationProperties ---
            var authProps = new AuthenticationProperties();

            // --- Create TokenValidatedContext ---
            var tokenContext = new TokenValidatedContext(
                httpContext,
                scheme,
                new OpenIdConnectOptions(),
                principal,
                authProps
            );

            // --- Act ---
            await AuthEventHandlers.AuditLoginAsync(tokenContext);

            // --- Assert ---
            var user = await db.Users.Include(u => u.Role)
                                     .FirstOrDefaultAsync(u => u.Email == email);
            user.Should().NotBeNull();
            user!.ExternalId.Should().Be(externalId);
            user.Role.Should().NotBeNull();
            user.Role.Name.Should().Be("BasicUser");

            var loginEvent = await db.SecurityEvents.FirstOrDefaultAsync();
            loginEvent.Should().NotBeNull();
            loginEvent!.EventType.Should().Be("LoginSuccess");
            loginEvent.AuthorUserId.Should().Be(user.Id);
            loginEvent.AffectedUserId.Should().Be(user.Id);
        }

        [Fact]
        public async Task AuditLogoutAsync_ShouldAddLogoutSuccessEvent()
        {
            // --- Setup in-memory EF DbContext ---
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("AuditLogoutTestDb")
                .Options;

            using var db = new AppDbContext(options);

            // --- Seed user ---
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "jane@example.com",
                ExternalId = "sub-123",
                RoleId = Guid.NewGuid(),
                Role = new Roles { Id = Guid.NewGuid(), Name = "BasicUser" }
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            // --- Setup HttpContext with DI ---
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
            new Claim("email", user.Email)
            }, "TestAuth")),
                RequestServices = new ServiceCollection()
                    .AddSingleton(db)
                    .BuildServiceProvider()
            };

            // --- Setup AuthenticationScheme and AuthenticationProperties ---
            var scheme = new AuthenticationScheme("TestScheme", "TestScheme", typeof(OpenIdConnectHandler));
            var authProps = new AuthenticationProperties();

            // --- Create RedirectContext ---
            var redirectContext = new RedirectContext(
                httpContext,
                scheme,
                new OpenIdConnectOptions(),
                authProps
            );

            // --- Act ---
            await AuthEventHandlers.AuditLogoutAsync(redirectContext);

            // --- Assert ---
            var logoutEvent = await db.SecurityEvents.FirstOrDefaultAsync();
            logoutEvent.Should().NotBeNull();
            logoutEvent!.EventType.Should().Be("LogoutSuccess");
            logoutEvent.AuthorUserId.Should().Be(user.Id);
            logoutEvent.AffectedUserId.Should().Be(user.Id);
        }

    
    }
}
