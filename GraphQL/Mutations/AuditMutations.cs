using GraphQL.Services;

namespace GraphQL.Mutations
{
    [ExtendObjectType(typeof(Mutation))]
    public class AuditMutations
    {
        

        // Inject the service through constructor
        

        public async Task<string> AuditLogin(AuditDto input, [Service] IAuditService auditService)
        {
            await auditService.AuditLoginAsync(input.Email, input.ExternalId, input.Provider);
            return "Login audit recorded";
        }

        public async Task<string> AuditLogout(AuditDto input, [Service] IAuditService auditService)
        {
            await auditService.AuditLogoutAsync(input.Email, input.ExternalId);
            return "Logout audit recorded";
        }
    }

// DTO remains the same
public record AuditDto(string Email, string ExternalId, string? Provider = null);
}