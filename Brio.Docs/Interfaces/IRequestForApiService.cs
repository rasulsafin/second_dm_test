using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Client;

namespace Brio.Docs.Interfaces
{
    public interface IRequestForApiService
    {
        /// <summary>
        /// Adds request to the queue.
        /// </summary>
        /// <param name="id">Id of the long task.</param>
        /// <param name="task">Task to be queued.</param>
        /// <param name="src">Cancellation token source for cancellation.</param>
        void AddRequest(string id, Task<RequestResult> task, CancellationTokenSource src);

        /// <summary>
        /// Sets new progress value to the request.
        /// </summary>
        /// <param name="value">Progress value.</param>
        /// <param name="id">Id of the long task.</param>
        void SetProgress(double value, string id);
    }
}
