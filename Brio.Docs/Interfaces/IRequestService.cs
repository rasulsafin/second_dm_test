using System.Threading;
using System.Threading.Tasks;

namespace Brio.Docs.Client.Services
{
    /// <summary>
    /// Service for managing long tasks on server side.
    /// </summary>
    public interface IRequestService
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
