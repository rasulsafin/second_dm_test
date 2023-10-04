using Brio.Docs.External.CloudBase.Synchronizers;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.GoogleDrive.Synchronization
{
    public class GoogleDriveConnectionContext : AConnectionContext
    {
        private GoogleDriveManager manager;

        private GoogleDriveConnectionContext()
        {
        }

        public static GoogleDriveConnectionContext CreateContext(GoogleDriveManager manager)
        {
            var context = new GoogleDriveConnectionContext { manager = manager };
            return context;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => new StorageObjectiveSynchronizer(manager);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => new StorageProjectSynchronizer(manager);
    }
}
