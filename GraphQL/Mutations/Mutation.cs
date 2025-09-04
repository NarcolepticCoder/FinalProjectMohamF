using Data.Dtos;
using GraphQL.Services;
using HotChocolate.Authorization;

namespace GraphQL.Mutations
{
public class Mutation {
    
    
    [Authorize(Policy = "CanViewRoleChanges")]
    public async Task<AssignRoleResultDto> AssignUserRole(
        Guid userId,
        Guid roleId,
        Guid authorUserId, [Service] IUserService userService 
        )
    {
        
        return await userService.AssignUserRoleAsync(userId, roleId, authorUserId);
    }
}
}