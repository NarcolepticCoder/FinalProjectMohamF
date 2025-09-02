using Data.DTOs;

namespace GraphQL.Services
{
    public interface IUserService
    {
        Task<AssignRoleResult> AssignUserRoleAsync(
        Guid affectedUserId, Guid newRoleId, Guid currentUserId);
        Task<List<ClaimDto>> GetUserClaimsAsync(string? externalId);
    }
}