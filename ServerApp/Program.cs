using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

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
      // Hook events for audit logging:
      o.Events = new OpenIdConnectEvents
      {
          OnTokenValidated = AuthEventHandlers.AuditLoginAsync,
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
      o.Events = new OpenIdConnectEvents
      {
          OnTokenValidated = AuthEventHandlers.AuditLoginAsync,
          OnRedirectToIdentityProviderForSignOut = AuthEventHandlers.AuditLogoutAsync
      };
  });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewAuthEvents",
        p => p.RequireClaim("permissions", "Audit.ViewAuthEvents"));
    options.AddPolicy("CanViewRoleChanges",
        p => p.RequireClaim("permissions", "Audit.ViewRoleChanges"));
});

var app = builder.Build();

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
