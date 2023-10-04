using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Brio.Docs.Updater
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var startup = new Startup(args);
            var hostBuilder = Host.CreateDefaultBuilder(args)
               .UseSerilog((_, _, configuration) => configuration.ReadFrom.Configuration(startup.Configuration))
               .ConfigureServices(startup.ConfigureServices);
            var host = hostBuilder.Build();
            using var scope = host.Services.CreateScope();
            var migrator = scope.ServiceProvider.GetRequiredService<Migrator>();
            await migrator.UpdateDatabase();
        }
    }
}
