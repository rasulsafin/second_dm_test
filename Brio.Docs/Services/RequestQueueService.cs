using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Services;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Extensions;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Services
{
    public class RequestQueueService : IRequestQueueService, IRequestService
    {
        private static readonly Dictionary<string, Request> QUEUE
            = new Dictionary<string, Request>();

        private readonly ILogger<RequestQueueService> logger;

        public RequestQueueService(ILogger<RequestQueueService> logger)
        {
            this.logger = logger;
            logger.LogTrace("RequestQueueService created");
        }

        public Task<double> GetProgress(string id)
        {
            logger.LogTrace("GetProgress {@LongRequestID}", id);
            var request = QUEUE.FindOrThrow(id);
            return Task.FromResult(request.Task.IsCompleted ? 1.0 : request.Progress);
        }

        public Task Cancel(string id)
        {
            logger.LogTrace("Cancel started for id = {@LongRequestID}", id);
            var job = QUEUE.FindOrThrow(id);
            job.Src.Cancel();
            logger.LogInformation("Operation {@LongRequestID} canceled", id);
            QUEUE.Remove(id);
            return Task.CompletedTask;
        }

        public Task<RequestResult> GetResult(string id)
        {
            logger.LogTrace("GetResult started for id = {@LongRequestID}", id);
            var job = QUEUE.FindOrThrow(id);

            if (job.Task.IsCompleted)
            {
                var result = job.Task.Result;
                logger.LogDebug("Result of the operation {@LongRequestID}: {@Result}", id, result);
                QUEUE.Remove(id);
                return Task.FromResult(result);
            }
            else
            {
                logger.LogWarning("Trying to get result of the incomplete operation {@LongRequestID}", id);
                throw new InvalidOperationException($"The job {id} is not finished yet");
            }
        }

        public void AddRequest(string id, Task<RequestResult> task, CancellationTokenSource src)
        {
            logger.LogTrace("AddRequest {@LongRequestID}", id);
            QUEUE.Add(id, new Request(task, src));
        }

        public void SetProgress(double value, string id)
        {
            logger.LogTrace("Progress {@LongRequestID} : {@Progress}", id, value);
            if (QUEUE.TryGetValue(id, out var job))
            {
                job.Progress = value;
                return;
            }
        }
    }
}
