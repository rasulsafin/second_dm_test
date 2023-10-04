using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Services;
using Brio.Docs.Client.Services.ForApi.Helpers;
using Brio.Docs.Interfaces;
using System.Threading;

namespace Brio.Docs.Services.ForApi
{
    public class SynchronizationForApiService : ISynchronizationForApiService
    {
        private readonly IHttpRequestForApiHandlerService httpService;
        private readonly IRequestForApiService requestQueue;

        public SynchronizationForApiService(IHttpRequestForApiHandlerService httpService, IRequestForApiService requestQueue) 
        {
            this.httpService = httpService;
            this.requestQueue = requestQueue;
        }

        public Task<IEnumerable<DateTime>> GetSynchronizationDates(ID<UserDto> userID)
        {
            return Task.FromResult<IEnumerable<DateTime>>(new List<DateTime> { DateTime.Now });
        }

        public Task<bool> RemoveAllSynchronizationDates(ID<UserDto> userID)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveLastSynchronizationDate(ID<UserDto> userID)
        {
            throw new NotImplementedException();
        }

        public async Task<RequestID> Synchronize(ID<UserDto> userID)
        {
            try
            {
                var id = Guid.NewGuid().ToString();

                var src = new CancellationTokenSource();

                var task = Task.Factory.StartNew(async () =>
                {
                    return new RequestResult(true);
                });

                requestQueue.AddRequest(id, task.Unwrap(), src);

                var response = await httpService.SendGetRequest("api/synchronization");

                return new RequestID(id);
            }
            catch
            {
                throw;
            }
        }
    }
}
