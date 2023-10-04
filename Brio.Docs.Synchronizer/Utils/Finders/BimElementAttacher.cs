using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utilities.Finders
{
    public class BimElementAttacher : IAttacher<BimElement>
    {
        private readonly ILogger<BimElementAttacher> logger;
        private readonly DMContext context;

        public BimElementAttacher(ILogger<BimElementAttacher> logger, DMContext context)
        {
            this.logger = logger;
            this.context = context;

            logger.LogTrace("BimElementAttacher created");
        }

        public IReadOnlyCollection<BimElement> RemoteCollection { get; set; }

        public async Task AttachExisting(SynchronizingTuple<BimElement> tuple)
        {
            logger.LogStartAction(tuple);
            if (tuple.Local != null && tuple.Synchronized != null)
                return;

            var parentName = tuple.GetPropertyValue(x => x.ParentName);
            var guid = tuple.GetPropertyValue(x => x.GlobalID);

            var found = await context.BimElements
               .FirstOrDefaultAsync(x => x.ParentName == parentName && x.GlobalID == guid)
               .ConfigureAwait(false);

            logger.LogDebug("Bim element found: #{Id}", found?.ID);

            if (found == null)
                return;

            bool Change(ref BimElement value)
            {
                if (value == null)
                {
                    value = found;
                    return true;
                }

                return false;
            }

            tuple.ForEachChange(Change);
        }
    }
}
