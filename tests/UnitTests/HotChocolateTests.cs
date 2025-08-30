using Data;
using Data.Entities;
using GraphQL;
using GraphQL.Mutations;
using GraphQL.Repositories;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Snapshooter.Xunit;
using UnitTests.Helpers;

namespace UnitTests
{
    public class HotChocolateTests
    {
        [Fact]

        public async Task QueryShouldReturnListOfUsers()
        {
            

            var executor = await TestSchemaBuilder.BuildAsync(seed: db =>
            {

                db.Users.Add(new User
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Email = "test123@shouldwork.com",
                    ExternalId = "123",
                    Role = new Roles { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Name = "Mo" },
                });
                db.Users.Add(new User
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Email = "test321@shouldwork.com",
                    ExternalId = "321",
                    Role = new Roles { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Name = "Works" },
                });


            });


            var result = await executor.ExecuteAsync(@"
            query {
            users {
            id
            email
            role {
                id
                name
                  }
                }
              }
            ");
            
            
            result.ToJson().MatchSnapshot();
        }
        
        [Fact]
        public async Task SchemaTest()
        {
            var executor = await TestSchemaBuilder.BuildAsync();

            //print schema from executor
            var sdl = executor.Schema.Print();

            sdl.ToString().MatchSnapshot();
            
            
        }
    }
}