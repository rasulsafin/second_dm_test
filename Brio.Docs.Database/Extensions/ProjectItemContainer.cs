using System.Threading.Tasks;
using Brio.Docs.Database.Models;

namespace Brio.Docs.Database
{
    public class ProjectItemContainer : IItemContainer
    {
        private readonly Project project;

        public ProjectItemContainer(Project project)
        {
            this.project = project;
        }

        public int ItemParentID => project.ID;

        public Task<bool> IsItemLinked(Item item)
        {
            return Task.FromResult(item.ProjectID == project.ID);
        }
    }
}
