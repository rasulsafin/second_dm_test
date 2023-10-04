using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Interfaces
{
    /// <summary>
    /// Represents an object for synchronizing a filtered database data with a remote connection data.
    /// </summary>
    public interface ISynchronizer
    {
        /// <summary>
        /// Synchronizes a filtered database data with a remote connection data.
        /// </summary>
        /// <param name="data">The settings for synchronizing.</param>
        /// <param name="connection">The remote connection.</param>
        /// <param name="info">The info of remote connection.</param>
        /// <param name="progress">The progress of the synchronization.</param>
        /// <param name="token">The token to cancel the operation.</param>
        /// <returns>A collection with errors that were received during synchronization.</returns>
        public Task<ICollection<SynchronizingResult>> Synchronize(
            SynchronizingData data,
            IConnection connection,
            ConnectionInfoExternalDto info,
            IProgress<double> progress,
            CancellationToken token);
    }
}
