using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Services;
using Brio.Docs.Client.Services.ForApi;
using Brio.Docs.Client.Services.ForApi.Helpers;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Interfaces;

namespace Brio.Docs.Services.ForApi
{
    public class ConnectionForApiService : IConnectionForApiService
    {
        private readonly IRequestForApiService requestQueue;

        public ConnectionForApiService(IRequestForApiService requestQueue)
        {
            this.requestQueue = requestQueue;
        }

        private IHttpRequestForApiHandlerService httpService;

        public ConnectionForApiService(IHttpRequestForApiHandlerService httpService, IRequestForApiService requestQueue)
        {
            this.requestQueue = requestQueue;
            this.httpService = httpService;
        }

        public Task<ID<ConnectionInfoDto>> Add(ConnectionInfoToCreateDto connectionInfo)
        {
            throw new NotImplementedException();
        }

        public async Task<RequestID> Connect(ID<UserDto> userID)
        {
            var id = Guid.NewGuid().ToString();
            var src = new CancellationTokenSource();

            var task = Task.Factory.StartNew(async () =>
            {
                return new RequestResult(new ConnectionStatusDto() { Status = RemoteConnectionStatus.OK, Message = "Good", });
            });

            requestQueue.AddRequest(id, task.Unwrap(), src);

            return new RequestID(id);
        }

        public Task<ConnectionInfoDto> Get(ID<UserDto> userID)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<EnumerationValueDto>> GetEnumerationVariants(ID<UserDto> userID, ID<EnumerationTypeDto> enumerationTypeID)
        {
            throw new NotImplementedException();
        }

        public Task<ConnectionStatusDto> GetRemoteConnectionStatus(ID<UserDto> userID)
        {
            var connectionStatus = new ConnectionStatusDto()
            {
                Status = RemoteConnectionStatus.OK,
                Message = "Good",
            };

            return Task.FromResult(connectionStatus);
        }
    }
}
