namespace UnitTests
{
    using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
    using Data.Entities;
    using GraphQL.Repositories;
    using GraphQL.Services;
    using Moq;
using Xunit;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _service = new UserService(_repoMock.Object);
    }

    [Fact]
    public async Task GetUserClaimsAsync_ReturnsEmpty_WhenExternalIdIsNull()
    {
        var result = await _service.GetUserClaimsAsync(null);

        Assert.Empty(result);
        _repoMock.Verify(r => r.GetUserWithRoleAndClaimsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetUserClaimsAsync_ReturnsEmpty_WhenUserNotFound()
    {
        _repoMock.Setup(r => r.GetUserWithRoleAndClaimsAsync("123"))
                 .ReturnsAsync((User)null);

        var result = await _service.GetUserClaimsAsync("123");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserClaimsAsync_ReturnsOnlyRole_WhenNoRoleClaims()
    {
        var user = new User
        {
            ExternalId = "123",
            Role = new Roles
            {
                Name = "Admin",
                RoleClaim = new List<RoleClaims>()
            }
        };

        _repoMock.Setup(r => r.GetUserWithRoleAndClaimsAsync("123"))
                 .ReturnsAsync(user);

        var result = await _service.GetUserClaimsAsync("123");

        Assert.Single(result);
        Assert.Equal(ClaimTypes.Role, result[0].Type);
        Assert.Equal("Admin", result[0].Value);
    }

    [Fact]
    public async Task GetUserClaimsAsync_ReturnsRoleAndRoleClaims()
    {
        var user = new User
        {
            ExternalId = "123",
            Role = new Roles
            {
                Name = "Admin",
                RoleClaim = new List<RoleClaims>
                {
                    new RoleClaims { Claim = new Claims { Type = "Permission", Value = "Read" } },
                    new RoleClaims { Claim = new Claims { Type = "Permission", Value = "Write" } }
                }
            }
        };

        _repoMock.Setup(r => r.GetUserWithRoleAndClaimsAsync("123"))
                 .ReturnsAsync(user);

        var result = await _service.GetUserClaimsAsync("123");

        Assert.Equal(3, result.Count); // role + 2 claims
        Assert.Contains(result, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        Assert.Contains(result, c => c.Type == "Permission" && c.Value == "Read");
        Assert.Contains(result, c => c.Type == "Permission" && c.Value == "Write");
    }
}

}