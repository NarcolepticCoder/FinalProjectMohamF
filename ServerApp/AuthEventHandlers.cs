using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Net.Http.Json;
using System.Security.Claims;

public static class AuthEventHandlers
{
    public static async Task AuditLoginAsync(TokenValidatedContext context)
    {
        var factory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
        var httpClient = factory.CreateClient("ApiClient");

        // ✅ Email fallback chain
        var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value
          ?? context.Principal?.FindFirst("preferred_username")?.Value
          ?? context.Principal?.Identity?.Name
          ?? throw new InvalidOperationException("No usable identifier claim found.");

        // ✅ ExternalId from NameIdentifier (instead of "sub")
        var externalId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
          ?? throw new InvalidOperationException("No NameIdentifier claim found.");
        
        var provider = context.Scheme?.Name ?? "Unknown";

        
        var mutation = new
        {
            query = @" mutation($input: AuditDtoInput!) 
        { auditLogin(input: $input) }"
        ,
            variables = new { input = new { Email = email, ExternalId = externalId, Provider = provider } }
        };
        await httpClient.PostAsJsonAsync("graphql", mutation);

    }

    public static async Task AuditLogoutAsync(RedirectContext context)
    {
        var factory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
        var httpClient = factory.CreateClient("ApiClient");

        // ✅ Email fallback chain
        var email = context.HttpContext.User?.FindFirst(ClaimTypes.Email)?.Value
                 ?? context.HttpContext.User?.FindFirst("preferred_username")?.Value
                 ?? context.HttpContext.User?.Identity?.Name;

        // ✅ ExternalId from NameIdentifier if available
        var externalId = context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        if (string.IsNullOrEmpty(email))
            return;

        
        var mutation = new
        {
            query = @" mutation($input: AuditDtoInput!) 
        { auditLogout(input: $input) }"
        ,
            variables = new { input = new { Email = email, ExternalId = externalId } }
        };
        await httpClient.PostAsJsonAsync("graphql", mutation);


    }
    public static async Task AuditLogoutLocalAsync(HttpContext context)
    {
        var factory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
        var httpClient = factory.CreateClient("ApiClient");

        var email = context.User?.FindFirst(ClaimTypes.Email)?.Value
                    ?? context.User?.FindFirst("preferred_username")?.Value
                    ?? context.User?.Identity?.Name;

        var externalId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        if (string.IsNullOrEmpty(email))
            return;

        

        var mutation = new
        {
            query = @" mutation($input: AuditDtoInput!) 
        { auditLogout(input: $input) }"
        ,
            variables = new { input = new { Email = email, ExternalId = externalId } }
        };
        await httpClient.PostAsJsonAsync("graphql", mutation);
    }
}
