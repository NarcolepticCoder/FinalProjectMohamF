using GraphQL.Services;

namespace GraphQL
{
    public class Mutation
{
    public async Task<AssignRoleResult> AssignUserRole(
        Guid userId,
        Guid roleId,
        Guid authorUserId, // you’ll later swap this to come from HttpContext.User
        [Service] UserService userService)
    {
        return await userService.AssignUserRoleAsync(userId, roleId, authorUserId);
    }
}
}