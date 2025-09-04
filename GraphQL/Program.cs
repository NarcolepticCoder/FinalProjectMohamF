using System.Text;
using Data;
using GraphQL;
using GraphQL.Mutations;
using GraphQL.Repositories;
using GraphQL.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddScoped<IAuditService,AuditService>()
    .AddScoped<IUserService,UserService>()
    .AddScoped<Mutation>()           // root type instance
    .AddScoped<AuditMutations>()
    .AddScoped<IUserRepository, UserRepository>()
    .AddScoped<IAuditRepository, AuditRepository>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", policy =>
        {
            policy.WithOrigins("http://serverapp") // frontend URLs
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "BlazorServer", // must match LocalTokenService issuer

            ValidateAudience = true,
            ValidAudience = "MyApi", // must match LocalTokenService audience

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["LocalToken:SigningKey"]!)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
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

builder.Services
    .AddGraphQLServer()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<AuditMutations>()
    .AddProjections()
    .AddFiltering()
    .DisableIntrospection(false)
    .AddSorting();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}
app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();


app.MapGraphQL();

app.Run();
