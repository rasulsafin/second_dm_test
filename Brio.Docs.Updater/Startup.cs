using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Brio.Docs.Updater
{
    public class Startup
    {
        public Startup(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");

#if DEBUG
            builder.AddJsonFile("appsettings.Debug.json");
#endif

            builder.AddCommandLine(args);

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        private string ConnectionString
            => Configuration.GetConnectionString("DefaultConnection");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Database.DMContext>(options => options.UseSqlite(ConnectionString));
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
            services.AddTransient<Migrator>();
        }
    }
}
