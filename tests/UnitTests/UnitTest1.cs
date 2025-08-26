using Data.Entities;
using GraphQL.Repositories;
using GraphQL.Services;
using Moq;
namespace UnitTests
{

    public class UnitTest1
    {
        [Fact]
        public async Task UserRoleAssignedShouldWrite_RoleAssignedEventOnSave()
        {
            // --- Arrange ---
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
                    .Returns(Task.CompletedTask);

            var service = new UserService(mockRepo.Object);

            // --- Act ---
            await service.AssignUserRoleAsync(affectedUserId, newRole.Id, authorUserId);

            // --- Assert ---
            Assert.Equal(newRole.Id, affectedUser.RoleId);
            Assert.Equal(newRole.Name, affectedUser.Role.Name);

            mockRepo.Verify(r => r.AddSecurityEventAsync(
                It.Is<SecurityEvents>(e =>
                    e.AuthorUserId == authorUserId &&
                    e.AffectedUserId == affectedUserId &&
                    e.EventType == "RoleAssigned" &&
                    e.Details == $"from={oldRole.Name} to={newRole.Name}"
                )
            ), Times.Once);
    


        }
    }
}
