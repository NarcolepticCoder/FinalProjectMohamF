using GraphQL.Services;

namespace GraphQL.Mutations
{
public class Mutation {
    private readonly UserService _userService;
    public Mutation(UserService userService)
    {
        _userService = userService;
    }

    public async Task<AssignRoleResult> AssignUserRole(
        Guid userId,
        Guid roleId,
        Guid authorUserId // you’ll later swap this to come from HttpContext.User
        )
    {
        return await _userService.AssignUserRoleAsync(userId, roleId, authorUserId);
    }
}
}