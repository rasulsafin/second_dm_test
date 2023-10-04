using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Services;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.HttpConnection.Services
{
    internal class ConnectionService : ServiceBase, IConnectionService
    {
        private static readonly string PATH = "Connections";

        public ConnectionService(Connection connection)
            : base(connection)
        {
        }

        public async Task<ID<ConnectionInfoDto>> Add(ConnectionInfoToCreateDto connectionInfo)
            => await Connection.PostObjectJsonAsync<ConnectionInfoToCreateDto, ID<ConnectionInfoDto>>($"{PATH}", connectionInfo);

        public async Task<RequestID> Connect(ID<UserDto> userID)
            => await Connection.GetDataAsync<RequestID>($"{PATH}/connect/{{0}}", userID);

        public async Task<ConnectionInfoDto> Get(ID<UserDto> userID)
            => await Connection.GetDataAsync<ConnectionInfoDto>($"{PATH}/{{0}}", userID);

        public async Task<IEnumerable<EnumerationValueDto>> GetEnumerationVariants(ID<UserDto> userID, ID<EnumerationTypeDto> enumerationTypeID)
            => await Connection.GetDataQueryAsync<IEnumerable<EnumerationValueDto>>($"{PATH}/enumerationValues",
                                                                                    $"userID={{0}}&enumerationTypeID={{1}}",
                                                                                    new object[] { userID, enumerationTypeID });

        public async Task<ConnectionStatusDto> GetRemoteConnectionStatus(ID<UserDto> userID)
            => await Connection.GetDataAsync<ConnectionStatusDto>($"{PATH}/status/{{0}}", userID);
    }
}
