using Brio.Docs.External.CloudBase.Synchronizers;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.YandexDisk.Synchronization
{
    public class YandexConnectionContext : AConnectionContext
    {
        private YandexManager manager;

        private YandexConnectionContext()
        {
        }

        public static YandexConnectionContext CreateContext(YandexManager manager)
        {
            var context = new YandexConnectionContext { manager = manager };
            return context;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => new StorageObjectiveSynchronizer(manager);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => new StorageProjectSynchronizer(manager);
    }
}
