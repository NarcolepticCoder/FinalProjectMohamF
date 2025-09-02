using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HotChocolate;
using HotChocolate.Execution;
using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Mutations;

namespace UnitTests.Helpers
{
    public static class TestSchemaBuilder
    {
        public static async Task<IRequestExecutor> BuildAsync(
            string? dbName = null,
            Action<AppDbContext>? seed = null)
        {
            // Unique DB per test
            var uniqueDbName = dbName ?? $"TestDb_{Guid.NewGuid()}";

            var services = new ServiceCollection();

            services
                .AddLogging()
                .AddAuthorization()
                .AddDbContext<AppDbContext>(opt =>
                    opt.UseInMemoryDatabase(uniqueDbName))
                .AddGraphQL()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddTypeExtension<AuditMutations>()
                .AddAuthorization()
                .AddProjections()
                .AddFiltering()
                .AddSorting();

            var sp = services.BuildServiceProvider();

            // Optional seeding
            if (seed != null)
            {
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                seed(db);
                await db.SaveChangesAsync();
            }

            var resolver = sp.GetRequiredService<IRequestExecutorResolver>();
            return await resolver.GetRequestExecutorAsync();
        }
    }
}
