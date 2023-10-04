using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Interfaces
{
    /// <summary>
    /// Represents tool to synchronize data.
    /// </summary>
    internal interface ISynchronizerProcessor
    {
        /// <summary>
        /// Synchronizes all filtered data with each other.
        /// </summary>
        /// <param name="data">Synchronization parameters.</param>
        /// <param name="remoteCollection">The filtered collection of external entities to be synchronized.</param>
        /// <param name="set">The filtered db collection for synchronization.</param>
        /// <param name="token">The token to cancel operation.</param>
        /// <param name="progress">The progress of the operation.</param>
        /// <returns>The task of the operation with collection of failed information.</returns>
        /// <typeparam name="TDB">The type of entities for synchronization.</typeparam>
        /// <typeparam name="TDto">The data transfer model type of entities.</typeparam>
        Task<List<SynchronizingResult>> Synchronize<TDB, TDto>(
            SynchronizingData data,
            IEnumerable<TDB> remoteCollection,
            IQueryable<TDB> set,
            CancellationToken token,
            IProgress<double> progress = null)
            where TDB : class, ISynchronizable<TDB>, new();
    }
}
