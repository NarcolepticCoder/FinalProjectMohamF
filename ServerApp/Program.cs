using System.Security.Claims;
using Data;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using ServerApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddControllers();

builder.Services.AddSingleton<LocalTokenService>();
builder.Services.AddHttpContextAccessor();




builder.Services.AddHttpClient(ServerClient.ClientName, client =>

    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!))
    .AddHttpMessageHandler<ApiTokenHandler>();

builder.Services.AddServerClient();
builder.Services.AddTransient<ApiTokenHandler>();




builder.Services
  .AddAuthentication(options =>
  {
      options.DefaultScheme = "Cookies";
      options.DefaultChallengeScheme = "Okta"; // default button can be Okta
  })
  .AddCookie("Cookies", o => { o.SlidingExpiration = true; })
  .AddOpenIdConnect("Okta", o =>
  {
      o.Authority = builder.Configuration["Okta:Authority"]; // e.g., https://dev-xxx.okta.com/oauth2/default
      o.ClientId = builder.Configuration["Okta:ClientId"];
      o.ClientSecret = builder.Configuration["Okta:ClientSecret"];
      o.ResponseType = "code";
      o.UsePkce = true;
      o.SaveTokens = true;
      o.Scope.Clear();
      o.Scope.Add("openid"); o.Scope.Add("profile"); o.Scope.Add("email");
      o.GetClaimsFromUserInfoEndpoint = true;
      o.CallbackPath = "/signin-oidc";
      // Hook events for audit logging:
      o.Events = new OpenIdConnectEvents
      {
          OnTokenValidated = ctx =>
          {
              var identity = (ClaimsIdentity)ctx!.Principal!.Identity!;
              identity.AddClaim(new Claim("idp", ctx.Scheme.Name)); // "Okta" or "Google"

              // Call your existing audit handler
              return AuthEventHandlers.AttachUserClaimsAsync(ctx);
          },
          OnRedirectToIdentityProviderForSignOut = AuthEventHandlers.AuditLogoutAsync
      };

  })
  .AddOpenIdConnect("Google", o =>
  {
      o.Authority = "https://accounts.google.com";
      o.ClientId = builder.Configuration["Google:ClientId"];
      o.ClientSecret = builder.Configuration["Google:ClientSecret"];
      o.ResponseType = "code";
      o.UsePkce = true;
      o.Scope.Clear();
      o.Scope.Add("openid"); o.Scope.Add("profile"); o.Scope.Add("email");
      o.GetClaimsFromUserInfoEndpoint = true;
      o.CallbackPath = "/signin-google";
      o.Events = new OpenIdConnectEvents
      {
          OnTokenValidated = ctx =>
          {
              var identity = (ClaimsIdentity)ctx!.Principal!.Identity!;
              identity.AddClaim(new Claim("idp", ctx.Scheme.Name)); // "Okta" or "Google"

              // Call your existing audit handler
              return AuthEventHandlers.AttachUserClaimsAsync(ctx);
              
          },
          
      };

  });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewAuthEvents",
        p => p.RequireClaim("permissions", "Audit.ViewAuthEvents"));
    options.AddPolicy("CanViewRoleChanges",
        p => p.RequireClaim("permissions", "Audit.RoleChanges"));
    //Policy for SecurityEvents to allow either permission
    options.AddPolicy("CanViewAnySecurityEvents", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("permissions", "Audit.ViewAuthEvents") ||
            ctx.User.HasClaim("permissions", "Audit.RoleChanges")));    
});


var app = builder.Build();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
