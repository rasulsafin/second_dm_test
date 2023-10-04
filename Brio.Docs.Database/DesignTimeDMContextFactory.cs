using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Brio.Docs.Database
{
    public class DesignTimeDMContextFactory : IDesignTimeDbContextFactory<DMContext>
    {
        public DMContext CreateDbContext(string[] args)
        {
            var configDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "Brio.Docs.Api");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(configDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<DMContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlite(connectionString);

            return new DMContext(builder.Options);
        }
    }
}
