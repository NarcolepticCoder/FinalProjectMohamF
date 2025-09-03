using Data;
using GraphQL;
using GraphQL.Mutations;
using GraphQL.Repositories;
using GraphQL.Services;
using Microsoft.EntityFrameworkCore;


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

builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", policy =>
        {
            policy.WithOrigins("http://serverapp") // frontend URLs
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

builder.Services
    .AddGraphQLServer()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<AuditMutations>()
    .AddAuthorization()
    .AddProjections()
    .AddFiltering()
    .DisableIntrospection(true)
    .AddSorting();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}
app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();


app.MapGraphQL();

app.Run();
