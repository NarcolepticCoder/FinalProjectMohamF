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


            mockRepo.Verify(r => r.AddSecurityEventAsync(It.Is<SecurityEvents>(e =>
            e.EventType == "RoleAssigned" &&
            e.AuthorUserId == authorUserId &&
            e.AffectedUserId == affectedUserId &&
            e.Details == $"from={oldRole.Name} to={newRole.Name}")), Times.Once);

            
        }
        [Fact]
        public async Task AuditLoginAsync_ShouldCreateUserAndAddLoginEvent()
        {
            // --- Arrange ---
            var email = "jane@example.com";
            var externalId = "sub-123";
            var userId = Guid.NewGuid();
            var provider = "Okta";

            // Mock IUserRepository
            var repoMock = new Mock<IAuditRepository>();
        
            // Setup CreateUserAsync to return a user
            repoMock.Setup(r => r.CreateUserAsync(email, externalId))
                    .ReturnsAsync(new Data.Entities.User
                    {
                        Id = userId,
                        Email = email,
                        ExternalId = externalId
                        
                    });

        
            repoMock.Setup(r => r.AddAuditEventAsync(It.IsAny<Data.Entities.SecurityEvents>()))
                    .Returns(Task.CompletedTask);

            // Instantiate the service
            var service = new AuditService(repoMock.Object);

            // --- Act ---
            await service.AuditLoginAsync(email, externalId, provider);

            // --- Assert ---
            repoMock.Verify(r => r.CreateUserAsync(email, externalId), Times.Once);

            repoMock.Verify(r => r.AddAuditEventAsync(
                It.Is<Data.Entities.SecurityEvents>(e =>
                    e.EventType == "LoginSuccess" &&
                    e.Details == $"provider={provider}" &&
                    e.AuthorUserId == userId &&
                    e.AffectedUserId == userId
                )), Times.Once);
        }


        [Fact]
        public async Task AuditLogoutAsync_ShouldAddLogoutAuditSuccessEvent()
        {
            // --- Arrange ---
            var email = "jane@example.com";
            var externalId = "sub-1234";
            var userId = Guid.NewGuid();

            // Mock IUserRepository
            var repoMock = new Mock<IAuditRepository>();
        
            // Setup CreateUserAsync to return a user
            repoMock.Setup(r => r.GetUserByEmailAsync(email))
                    .ReturnsAsync(new User
                    {
                        Id = userId,
                        Email = email,
                        ExternalId = externalId
                    });

        
            repoMock.Setup(r => r.AddAuditEventAsync(It.IsAny<Data.Entities.SecurityEvents>()))
                    .Returns(Task.CompletedTask);

            // Instantiate the service
            var service = new AuditService(repoMock.Object);
            // --- Act ---
            

            await service.AuditLogoutAsync(email, externalId);


            repoMock.Verify(r => r.AddAuditEventAsync(
                It.Is<Data.Entities.SecurityEvents>(e =>
                    e.EventType == "LogoutSuccess" &&
                    e.Details == "local sign out" &&
                    e.AuthorUserId == userId &&
                    e.AffectedUserId == userId
                )), Times.Once);
        }

    
    }
}
