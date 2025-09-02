using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace ServerApp.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        public IActionResult SignIn(string provider, string returnUrl = "/")
        {
            if (User.Identity!.IsAuthenticated)
                return LocalRedirect(returnUrl);

            return Challenge(
                new AuthenticationProperties { RedirectUri = returnUrl },
                provider // "Okta" or "Google"
            );
        }

        public async Task<IActionResult> SignOutAsync(string returnUrl = "/")
        {
            if (!User.Identity!.IsAuthenticated)
                return LocalRedirect(returnUrl);

            // Determine provider from claims
            var provider = User.Claims.FirstOrDefault(c => c.Type == "idp")?.Value;

            if (provider == "Google")
            {
                // Google: just clear local cookie, then audit logout
                await AuthEventHandlers.AuditLogoutLocalAsync(HttpContext);

                return SignOut(
                    new AuthenticationProperties { RedirectUri = returnUrl },
                    "Cookies"
                );
            }

            // Default: Okta or other OIDC provider
            // This triggers OnRedirectToIdentityProviderForSignOut, where your existing audit runs
            return SignOut(
                new AuthenticationProperties { RedirectUri = returnUrl },
                "Okta",    // provider-specific scheme
                "Cookies"  // always clear local cookie
                );
        
        }





    }
}
