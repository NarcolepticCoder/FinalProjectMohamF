using GraphQL.Services;

namespace GraphQL.Mutations
{
public class Mutation {
    
    

    public async Task<AssignRoleResult> AssignUserRole(
        Guid userId,
        Guid roleId,
        Guid authorUserId, [Service] IUserService userService // you’ll later swap this to come from HttpContext.User
        )
    {
        return await userService.AssignUserRoleAsync(userId, roleId, authorUserId);
    }
}
}