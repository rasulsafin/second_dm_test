using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Services;

namespace Brio.Docs.HttpConnection.Services
{
    internal class BimElementService : ServiceBase, IBimElementService
    {
        private static readonly string PATH = "BimElements";

        public BimElementService(Connection connection)
            : base(connection)
        {
        }

        public async Task<IEnumerable<BimElementStatusDto>> GetBimElementsStatuses(ID<ProjectDto> projectID)
            => await Connection.GetDataQueryAsync<IEnumerable<BimElementStatusDto>>($"{PATH}", $"projectID={{0}}", new object[] { projectID });
    }
}
