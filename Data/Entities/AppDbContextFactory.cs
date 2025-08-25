using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Hard-coded connection string for design-time migrations
            optionsBuilder.UseSqlServer(
                "Server=localhost,1434;Database=CommunicationDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}