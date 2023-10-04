using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Services;

namespace Brio.Docs.HttpConnection.Services
{
    internal class SynchronizationService : ServiceBase, ISynchronizationService
    {
        private static readonly string PATH = "Synchronizations";

        public SynchronizationService(Connection connection)
            : base(connection)
        {
        }

        public async Task<RequestID> Synchronize(ID<UserDto> userID)
            => await Connection.GetDataAsync<RequestID>($"{PATH}/start/{{0}}", userID);

        public async Task<IEnumerable<DateTime>> GetSynchronizationDates(ID<UserDto> userID)
            => await Connection.GetDataAsync<IEnumerable<DateTime>>($"{PATH}/dates/{{0}}", userID);

        public async Task<bool> RemoveLastSynchronizationDate(ID<UserDto> userID)
            => await Connection.DeleteDataAsync<bool>($"{PATH}/dates/{{0}}/last", userID);

        public async Task<bool> RemoveAllSynchronizationDates(ID<UserDto> userID)
            => await Connection.DeleteDataAsync<bool>($"{PATH}/dates/{{0}}/all", userID);
    }
}
