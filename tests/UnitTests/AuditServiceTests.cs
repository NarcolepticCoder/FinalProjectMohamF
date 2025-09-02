namespace UnitTests
{
    using System;
using System.Threading.Tasks;
    using Data.Entities;
    using GraphQL.Repositories;
    using GraphQL.Services;
    using Moq;
using Xunit;

public class AuditServiceTests
{
    private readonly Mock<IAuditRepository> _repoMock;
    private readonly AuditService _service;

    public AuditServiceTests()
    {
        _repoMock = new Mock<IAuditRepository>();
        _service = new AuditService(_repoMock.Object);
    }

    // -------- AuditLoginAsync --------

    [Fact]
    public async Task AuditLoginAsync_DoesNothing_WhenEmailIsNull()
    {
        await _service.AuditLoginAsync(null, "ext123", "Okta");

        _repoMock.Verify(r => r.GetUserByEmailAsync(It.IsAny<string>()), Times.Never);
        _repoMock.Verify(r => r.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _repoMock.Verify(r => r.AddAuditEventAsync(It.IsAny<SecurityEvents>()), Times.Never);
    }

    [Fact]
    public async Task AuditLoginAsync_CreatesAudit_WhenUserExists()
    {
        var existingUser = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
        _repoMock.Setup(r => r.GetUserByEmailAsync("test@example.com"))
                 .ReturnsAsync(existingUser);

        await _service.AuditLoginAsync("test@example.com", "ext123", "Okta");

        _repoMock.Verify(r => r.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _repoMock.Verify(r => r.AddAuditEventAsync(It.Is<SecurityEvents>(e =>
            e.EventType == "LoginSuccess" &&
            e.Details == "provider=Okta" &&
            e.AuthorUserId == existingUser.Id &&
            e.AffectedUserId == existingUser.Id
        )), Times.Once);
    }

    [Fact]
    public async Task AuditLoginAsync_CreatesUser_WhenUserDoesNotExist()
    {
        var newUser = new User { Id = Guid.NewGuid(), Email = "new@example.com" };

        _repoMock.Setup(r => r.GetUserByEmailAsync("new@example.com"))
                 .ReturnsAsync((User)null);
        _repoMock.Setup(r => r.CreateUserAsync("new@example.com", "ext456"))
                 .ReturnsAsync(newUser);

        await _service.AuditLoginAsync("new@example.com", "ext456", "Google");

        _repoMock.Verify(r => r.CreateUserAsync("new@example.com", "ext456"), Times.Once);
        _repoMock.Verify(r => r.AddAuditEventAsync(It.Is<SecurityEvents>(e =>
            e.EventType == "LoginSuccess" &&
            e.Details == "provider=Google" &&
            e.AuthorUserId == newUser.Id
        )), Times.Once);
    }

    // -------- AuditLogoutAsync --------

    [Fact]
    public async Task AuditLogoutAsync_DoesNothing_WhenEmailIsNull()
    {
        await _service.AuditLogoutAsync(null, "ext123");

        _repoMock.Verify(r => r.GetUserByEmailAsync(It.IsAny<string>()), Times.Never);
        _repoMock.Verify(r => r.AddAuditEventAsync(It.IsAny<SecurityEvents>()), Times.Never);
    }

    [Fact]
    public async Task AuditLogoutAsync_DoesNothing_WhenUserNotFound()
    {
        _repoMock.Setup(r => r.GetUserByEmailAsync("ghost@example.com"))
                 .ReturnsAsync((User)null);

        await _service.AuditLogoutAsync("ghost@example.com", "ext999");

        _repoMock.Verify(r => r.AddAuditEventAsync(It.IsAny<SecurityEvents>()), Times.Never);
    }

    [Fact]
    public async Task AuditLogoutAsync_CreatesAudit_WhenUserExists()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
        _repoMock.Setup(r => r.GetUserByEmailAsync("test@example.com"))
                 .ReturnsAsync(user);

        await _service.AuditLogoutAsync("test@example.com", "ext123");

        _repoMock.Verify(r => r.AddAuditEventAsync(It.Is<SecurityEvents>(e =>
            e.EventType == "LogoutSuccess" &&
            e.Details == "local sign out" &&
            e.AuthorUserId == user.Id &&
            e.AffectedUserId == user.Id
        )), Times.Once);
    }
}

}