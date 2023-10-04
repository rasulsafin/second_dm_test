using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.External.CloudBase.Synchronizers
{
    public class StorageProjectSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly ICloudManager manager;
        private List<ObjectiveExternalDto> objectives;
        private List<ProjectExternalDto> projects;

        public StorageProjectSynchronizer(ICloudManager manager)
            => this.manager = manager;

        public async Task<ProjectExternalDto> Add(ProjectExternalDto project)
        {
            var newId = Guid.NewGuid().ToString();
            project.ExternalID = newId;
            var createdProject = await PushProject(project, newId);

            return createdProject;
        }

        public async Task<IReadOnlyCollection<ProjectExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            await CheckCashedElements();
            return projects.Where(p => ids.Contains(p.ExternalID)).ToList();
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await CheckCashedElements();
            return projects.Where(p => p.UpdatedAt >= date).Select(p => p.ExternalID).ToList();
        }

        public async Task<ProjectExternalDto> Remove(ProjectExternalDto project)
        {
            var objectivesPath = PathManager.GetTableDir(nameof(ObjectiveExternalDto));
            objectives ??= await manager.PullAll<ObjectiveExternalDto>(objectivesPath);

            foreach (var objective in objectives.Where(x => x.ProjectExternalID == project.ExternalID))
                await manager.Delete<ObjectiveExternalDto>(objective.ExternalID);

            var deleteResult = await manager.Delete<ProjectExternalDto>(project.ExternalID);
            if (!deleteResult)
                return null;

            return project;
        }

        public async Task<ProjectExternalDto> Update(ProjectExternalDto project)
        {
            var updated = await PushProject(project);
            return updated;
        }

        private async Task<ProjectExternalDto> PushProject(ProjectExternalDto project, string newId = null)
        {
            newId ??= project.ExternalID;
            await ItemsSyncHelper.UploadFiles(project.Items, manager, project.Title);
            project.Items = null;
            UpdatedTimeUtilities.UpdateTime(project);
            var createSuccess = await manager.Push(project, newId);
            if (!createSuccess)
                return null;

            var createdProject = await manager.Pull<ProjectExternalDto>(newId);
            createdProject.Items = await ItemsSyncHelper.GetProjectItems(createdProject.Title, manager);
            return createdProject;
        }

        private async Task CheckCashedElements()
        {
            void UpdateTimeIfNeeded(ProjectExternalDto project, DateTime value)
            {
                if (project.UpdatedAt < value)
                    project.UpdatedAt = value;
            }

            if (projects != null)
                return;

            projects = await manager.PullAll<ProjectExternalDto>(
                PathManager.GetTableDir(nameof(ProjectExternalDto)));

            foreach (var project in projects)
            {
                project.Items = await ItemsSyncHelper.GetProjectItems(project.Title, manager);
                var folderUpdatedAt = await ProjectHelper.GetItemsDirectoryUpdatedTime(project.Title, manager);
                var lastItemUpdatedAt = project.Items != null && project.Items.Any()
                    ? project.Items.Max(x => x.UpdatedAt)
                    : default;
                UpdateTimeIfNeeded(project, folderUpdatedAt);
                UpdateTimeIfNeeded(project, lastItemUpdatedAt);
            }
        }
    }
}
