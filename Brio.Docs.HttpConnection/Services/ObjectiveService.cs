using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Filters;
using Brio.Docs.Client.Services;
using Brio.Docs.Client.Sorts;

namespace Brio.Docs.HttpConnection.Services
{
    internal class ObjectiveService : ServiceBase, IObjectiveService
    {
        private static readonly string PATH = "Objectives";

        public ObjectiveService(Connection connection)
            : base(connection)
        {
        }

        public async Task<ObjectiveToListDto> Add(ObjectiveToCreateDto data)
            => await Connection.PostObjectJsonAsync<ObjectiveToCreateDto, ObjectiveToListDto>($"{PATH}", data);

        public async Task<bool> Update(ObjectiveDto projectData)
            => await Connection.PutObjectJsonAsync<ObjectiveDto, bool>($"{PATH}", projectData);

        public async Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID)
            => await Connection.GetDataAsync<ObjectiveDto>($"{PATH}/{{0}}", objectiveID);

        public async Task<PagedListDto<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectID, ObjectiveFilterParameters filter, SortParameters sort)
            => await Connection.PostObjectJsonQueryAsync<ObjectiveFilterParameters, PagedListDto<ObjectiveToListDto>>(
                $"{PATH}/project/{{0}}",
                query: "sort={0}",
                queryParams: new object[] { sort.ToQueryString() },
                data: filter,
                projectID);

        public async Task<IEnumerable<SubobjectiveDto>> GetObjectivesByParent(ID<ObjectiveDto> parentID)
            => await Connection.GetDataAsync<IEnumerable<SubobjectiveDto>>($"{PATH}/subobjectives/{{0}}", parentID);

        public async Task<IEnumerable<ObjectiveToSelectionDto>> GetObjectivesForSelection(ID<ProjectDto> projectID, ObjectiveFilterParameters filter)
            => await Connection.PostObjectJsonAsync<ObjectiveFilterParameters, IEnumerable<ObjectiveToSelectionDto>>(
                $"{PATH}/ids/{{0}}",
                data: filter,
                projectID);

        public async Task<IEnumerable<ID<ObjectiveDto>>> Remove(ID<ObjectiveDto> objectiveID)
            => await Connection.DeleteDataAsync<IEnumerable<ID<ObjectiveDto>>>($"{PATH}/{{0}}", objectiveID);

        public async Task<IEnumerable<ObjectiveToLocationDto>> GetObjectivesWithLocation(ID<ProjectDto> projectID, string itemName, ObjectiveFilterParameters filter)
            => await Connection.PostObjectJsonQueryAsync<ObjectiveFilterParameters, IEnumerable<ObjectiveToLocationDto>>(
                $"{PATH}/locations",
                $"projectID={{0}}&itemName={{1}}",
                new object[] { projectID, itemName },
                filter);

        public async Task<IEnumerable<ObjectiveBimParentDto>> GetParentsOfObjectivesBimElements(ID<ProjectDto> projectID)
            => await Connection.GetDataAsync<IEnumerable<ObjectiveBimParentDto>>($"{PATH}/bimparents/{{0}}", projectID);
    }
}
