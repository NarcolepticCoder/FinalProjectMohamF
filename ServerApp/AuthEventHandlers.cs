using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using ServerApp;

public static class AuthEventHandlers
{
    public static async Task AuditLoginAsync(TokenValidatedContext context)
    {
        var client = context.HttpContext.RequestServices.GetRequiredService<IServerClient>();

        // Email fallback
        var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value
          ?? context.Principal?.FindFirst("preferred_username")?.Value
          ?? context.Principal?.Identity?.Name
          ?? throw new InvalidOperationException("No usable identifier claim found.");

        var externalId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
          ?? throw new InvalidOperationException("No NameIdentifier claim found.");

        var provider = context.Scheme?.Name ?? "Unknown";

        await client.AuditLogin.ExecuteAsync(new AuditDtoInput
        {
            Email = email,
            ExternalId = externalId,
            Provider = provider
        });
    }

    public static async Task AuditLogoutAsync(RedirectContext context)
    {
        var client = context.HttpContext.RequestServices.GetRequiredService<IServerClient>();

        var email = context.HttpContext.User?.FindFirst(ClaimTypes.Email)?.Value
                 ?? context.HttpContext.User?.FindFirst("preferred_username")?.Value
                 ?? context.HttpContext.User?.Identity?.Name;

        if (string.IsNullOrEmpty(email))
            return;

        var externalId = context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        await client.AuditLogout.ExecuteAsync(new AuditDtoInput
        {
            Email = email,
            ExternalId = externalId
        });
    }

    public static async Task AuditLogoutLocalAsync(HttpContext context)
    {
        var client = context.RequestServices.GetRequiredService<IServerClient>();

        var email = context.User?.FindFirst(ClaimTypes.Email)?.Value
                 ?? context.User?.FindFirst("preferred_username")?.Value
                 ?? context.User?.Identity?.Name;

        if (string.IsNullOrEmpty(email))
            return;

        var externalId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        await client.AuditLogout.ExecuteAsync(new AuditDtoInput
        {
            Email = email,
            ExternalId = externalId
        });
    }

    public static async Task AttachUserClaimsAsync(TokenValidatedContext context)
    {
        var externalId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(externalId)) return;

        var client = context.HttpContext.RequestServices.GetRequiredService<IServerClient>();

        var result = await client.GetUserClaims.ExecuteAsync(externalId);

        if (result.Data?.UserClaims is null) return;

        var identity = (ClaimsIdentity)context.Principal!.Identity!;
        foreach (var claimDto in result.Data.UserClaims)
        {
            identity.AddClaim(new Claim(claimDto.Type, claimDto.Value));
        }

        identity.AddClaim(new Claim("idp", context.Scheme.Name));

        await AuditLoginAsync(context);
    }
}
