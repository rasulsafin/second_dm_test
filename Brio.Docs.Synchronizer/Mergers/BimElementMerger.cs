using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Mergers
{
    internal class BimElementMerger : IMerger<BimElement>
    {
        private readonly ILogger<BimElementMerger> logger;

        public BimElementMerger(ILogger<BimElementMerger> logger)
        {
            this.logger = logger;
            logger.LogTrace("BimElementMerger created");
        }

        public ValueTask Merge(SynchronizingTuple<BimElement> tuple)
        {
            logger.LogTrace(
                "Merge bim element started for tuple ({Local}, {Synchronized}, {Remote})",
                tuple.Local?.ID,
                tuple.Synchronized?.ID,
                tuple.Remote?.GlobalID);
            var first = tuple.AsEnumerable().First(x => x != null);

            bool CreateNew(ref BimElement synchronizableChild)
            {
                if (synchronizableChild == null)
                {
                    synchronizableChild = first;
                    return true;
                }

                return false;
            }

            tuple.ForEachChange(CreateNew);
            return ValueTask.CompletedTask;
        }
    }
}
