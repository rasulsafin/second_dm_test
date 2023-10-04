using Brio.Docs.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brio.Docs.Interfaces
{
    public interface IRequestQueueForApiService
    {
        /// <summary>
        /// Gets progress from 0 to 1 indicating Task completion status.
        /// </summary>
        /// <param name="id">Id of the task.</param>
        /// <returns>Progress from 0 to 1.</returns>
        Task<double> GetProgress(string id);

        /// <summary>
        /// Gets result of the complete task and destroys it.
        /// </summary>
        /// <param name="id">Id of the task.</param>
        /// <returns>Result of the completed task.</returns>
        Task<RequestResult> GetResult(string id);

        /// <summary>
        /// Cancels task.
        /// </summary>
        /// <param name="id">Id of the task.</param>
        /// <returns>Void task.</returns>
        Task Cancel(string id);
    }
}
