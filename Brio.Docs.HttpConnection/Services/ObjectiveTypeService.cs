using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Services;

namespace Brio.Docs.HttpConnection.Services
{
    internal class ObjectiveTypeService : ServiceBase, IObjectiveTypeService
    {
        private static readonly string PATH = "ObjectiveTypes";

        public ObjectiveTypeService(Connection connection)
            : base(connection)
        {
        }

        public async Task<ID<ObjectiveTypeDto>> Add(string typeName)
            => await Connection.PostObjectJsonAsync<string, ID<ObjectiveTypeDto>>($"{PATH}", typeName);

        public async Task<ObjectiveTypeDto> Find(ID<ObjectiveTypeDto> id)
            => await Connection.GetDataAsync<ObjectiveTypeDto>($"{PATH}/{{0}}", id);

        public async Task<ObjectiveTypeDto> Find(string typename)
            => await Connection.GetDataQueryAsync<ObjectiveTypeDto>($"{PATH}/name", $"typename={{0}}", new object[] { typename });

        public async Task<IEnumerable<ObjectiveTypeDto>> GetObjectiveTypes(ID<UserDto> id)
            => await Connection.GetDataAsync<IEnumerable<ObjectiveTypeDto>>($"{PATH}/list/{{0}}", id);

        public async Task<bool> Remove(ID<ObjectiveTypeDto> id)
             => await Connection.DeleteDataAsync<bool>($"{PATH}/{{0}}", id);
    }
}
