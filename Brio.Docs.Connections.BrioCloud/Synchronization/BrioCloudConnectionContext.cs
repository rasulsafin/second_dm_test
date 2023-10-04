using Brio.Docs.External.CloudBase.Synchronizers;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.BrioCloud.Synchronization
{
    public class BrioCloudConnectionContext : AConnectionContext
    {
        private readonly BrioCloudManager manager;

        public BrioCloudConnectionContext(BrioCloudManager manager)
        {
            this.manager = manager;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => new StorageObjectiveSynchronizer(manager);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => new StorageProjectSynchronizer(manager);
    }
}
