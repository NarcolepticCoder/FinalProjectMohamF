using Data.Dtos;
using GraphQL.Services;

namespace GraphQL.Mutations
{
    [ExtendObjectType(typeof(Mutation))]
    public class AuditMutations
    {

        public async Task<string> AuditLogin(AuditDto input, [Service] IAuditService auditService)
        {
            await auditService.AuditLoginAsync(input.Email, input.ExternalId, input.Provider!);
            return "Login audit recorded";
        }

        public async Task<string> AuditLogout(AuditDto input, [Service] IAuditService auditService)
        {
            await auditService.AuditLogoutAsync(input.Email, input.ExternalId);
            return "Logout audit recorded";
        }
    }
}

