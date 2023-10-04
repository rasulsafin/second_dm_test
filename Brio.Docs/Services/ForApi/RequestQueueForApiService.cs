using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Services;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Interfaces;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Extensions;

namespace Brio.Docs.Services.ForApi
{
    public class RequestQueueForApiService : IRequestQueueForApiService, IRequestForApiService
    {
        private static readonly Dictionary<string, Request> QUEUE
            = new Dictionary<string, Request>();

        public Task Cancel(string id)
        {
            var job = QUEUE.FindOrThrow(id);
            job.Src.Cancel();
            QUEUE.Remove(id);
            return Task.CompletedTask;
        }

        public Task<double> GetProgress(string id)
        {
            return Task.FromResult(1.0);
        }

        public Task<RequestResult> GetResult(string id)
        {
            var job = QUEUE.FindOrThrow(id);

            if (job.Task.IsCompleted)
            {
                var result = job.Task.Result;
                QUEUE.Remove(id);
                return Task.FromResult(result);
            }
            else
            {
                throw new InvalidOperationException($"The job {id} is not finished yet");
            }
        }

        public void AddRequest(string id, Task<RequestResult> task, CancellationTokenSource src)
        {
            QUEUE.Add(id, new Request(task, src));
        }

        public void SetProgress(double value, string id)
        {
            if (QUEUE.TryGetValue(id, out var job))
            {
                job.Progress = value;
                return;
            }
        }
    }
}
