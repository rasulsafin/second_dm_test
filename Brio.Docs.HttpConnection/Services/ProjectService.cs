using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Services;

namespace Brio.Docs.HttpConnection.Services
{
    internal class ProjectService : ServiceBase, IProjectService
    {
        private static readonly string PATH = "Projects";
        private static readonly string USER = "user";

        public ProjectService(Connection connection)
            : base(connection)
        {
        }

        public async Task<ProjectToListDto> Add(ProjectToCreateDto projectToCreate)
            => await Connection.PostObjectJsonAsync<ProjectToCreateDto, ProjectToListDto>($"{PATH}", projectToCreate);

        public async Task<ProjectDto> Find(ID<ProjectDto> projectID)
            => await Connection.GetDataAsync<ProjectDto>($"{PATH}/{{0}}", projectID);

        public async Task<IEnumerable<ProjectToListDto>> GetAllProjects()
            => await Connection.GetDataAsync<IEnumerable<ProjectToListDto>>($"{PATH}");

        public async Task<IEnumerable<ProjectToListDto>> GetUserProjects(ID<UserDto> userID)
            => await Connection.GetDataAsync<IEnumerable<ProjectToListDto>>($"{PATH}/{USER}/{{0}}", userID);

        public async Task<IEnumerable<UserDto>> GetUsers(ID<ProjectDto> projectID)
            => await Connection.GetDataAsync<IEnumerable<UserDto>>($"{PATH}/{{0}}/{USER}s", projectID);

        public async Task<bool> LinkToUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users)
            => await Connection.PostObjectJsonAsync<IEnumerable<ID<UserDto>>, bool>($"{PATH}/link/{{0}}", users, projectID);

        public async Task<bool> UnlinkFromUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users)
            => await Connection.PostObjectJsonAsync<IEnumerable<ID<UserDto>>, bool>($"{PATH}/unlink/{{0}}", users, projectID);

        public async Task<bool> Update(ProjectDto project)
            => await Connection.PutObjectJsonAsync<ProjectDto, bool>($"{PATH}", project);

        public async Task<bool> Remove(ID<ProjectDto> projectID)
            => await Connection.DeleteDataAsync<bool>($"{PATH}/{{0}}", projectID);
    }
}
