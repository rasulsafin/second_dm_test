using System.Threading.Tasks;
using Brio.Docs.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Updater
{
    internal class Migrator
    {
        private readonly DMContext context;
        private readonly ILogger<Migrator> logger;

        public Migrator(DMContext context, ILogger<Migrator> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        internal async Task UpdateDatabase()
        {
            logger.LogInformation("The database migration has started");
            await context.Database.MigrateAsync();
            logger.LogInformation("The database migration has been completed successfully");
        }
    }
}
