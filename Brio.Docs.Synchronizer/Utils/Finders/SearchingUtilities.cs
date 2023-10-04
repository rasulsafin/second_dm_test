using System.Threading.Tasks;
using Brio.Docs.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Synchronization.Utilities.Finders
{
    public static class SearchingUtilities
    {
        public static async Task<(Project localProject, Project syncProject)> GetProjectsByRemote(
            DbContext context,
            int? remoteProjectId)
        {
            var remoteProject = await context.Set<Project>().AsNoTracking()
               .Include(x => x.SynchronizationMate)
               .FirstOrDefaultAsync(x => x.ID == remoteProjectId)
               .ConfigureAwait(false);

            if (remoteProject == null)
                return default;

            Project localProject;
            Project syncProject;

            if (remoteProject.IsSynchronized)
            {
                localProject = await context.Set<Project>().AsNoTracking()
                   .FirstOrDefaultAsync(x => x.SynchronizationMateID == remoteProject.ID)
                   .ConfigureAwait(false);
                syncProject = remoteProject;
            }
            else
            {
                localProject = remoteProject;
                syncProject = localProject.SynchronizationMate;
            }

            return (localProject, syncProject);
        }
    }
}
