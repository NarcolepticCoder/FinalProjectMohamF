using GraphQL.Services;

namespace GraphQL.Mutations
{
    [ExtendObjectType(typeof(Mutation))]
    public class AuditMutations
    {
        private readonly AuditService _auditService;

        // Inject the service through constructor
        public AuditMutations(AuditService auditService)
        {
            _auditService = auditService;
        }

        public async Task<string> AuditLogin(AuditDto input)
        {
            await _auditService.AuditLoginAsync(input.Email, input.ExternalId, input.Provider);
            return "Login audit recorded";
        }

        public async Task<string> AuditLogout(AuditDto input)
        {
            await _auditService.AuditLogoutAsync(input.Email, input.ExternalId);
            return "Logout audit recorded";
        }
    }

// DTO remains the same
public record AuditDto(string Email, string ExternalId, string? Provider = null);
}