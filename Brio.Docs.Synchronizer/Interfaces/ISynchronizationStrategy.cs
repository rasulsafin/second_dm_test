using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Interfaces
{
    internal delegate Task<SynchronizingResult> SynchronizationFunc<TDB>(
        SynchronizingTuple<TDB> tuple,
        SynchronizingData data,
        CancellationToken token);

    /// <summary>
    /// The strategy to synchronize needed entity type.
    /// </summary>
    /// <typeparam name="TDB">The type of entities for synchronization.</typeparam>
    internal interface ISynchronizationStrategy<TDB>
        where TDB : class
    {
        /// <summary>
        /// Adds entity to local database. Saves synchronized state.
        /// </summary>
        /// <param name="tuple">The synchronization tuple.</param>
        /// <param name="data">Synchronization parameters.</param>
        /// <param name="token">The token to cancel operation.</param>
        /// <returns>The task of the operation with information about fails.</returns>
        Task<SynchronizingResult> AddToLocal(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            CancellationToken token);

        /// <summary>
        /// Adds entity to remote connection. Saves synchronized state.
        /// </summary>
        /// <param name="tuple">The synchronization tuple.</param>
        /// <param name="data">Synchronization parameters.</param>
        /// <param name="token">The token to cancel operation.</param>
        /// <returns>The task of the operation with information about fails.</returns>
        Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            CancellationToken token);

        /// <summary>
        /// Merges remote and local entities. Saves synchronized state.
        /// </summary>
        /// <param name="tuple">The synchronization tuple.</param>
        /// <param name="data">Synchronization parameters.</param>
        /// <param name="token">The token to cancel operation.</param>
        /// <returns>The task of the operation with information about fails.</returns>
        Task<SynchronizingResult> Merge(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            CancellationToken token);

        /// <summary>
        /// Orders unsorted entities.
        /// </summary>
        /// <param name="enumeration">All unsorted entities.</param>
        /// <returns>The sorted entities.</returns>
        public IEnumerable<TDB> Order(IEnumerable<TDB> enumeration);

        /// <summary>
        /// Removes entity from local database. Removes synchronized state.
        /// </summary>
        /// <param name="tuple">The synchronization tuple.</param>
        /// <param name="data">Synchronization parameters.</param>
        /// <param name="token">The token to cancel operation.</param>
        /// <returns>The task of the operation with information about fails.</returns>
        Task<SynchronizingResult> RemoveFromLocal(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            CancellationToken token);

        /// <summary>
        /// Removes entity from remote connection. Removes synchronized state.
        /// </summary>
        /// <param name="tuple">The synchronization tuple.</
        /// <param name="data">Synchronization parameters.</param>
        /// <param name="token">The token to cancel operation.</param>
        /// <returns>The task of the operation with information about fails.</returns>
        Task<SynchronizingResult> RemoveFromRemote(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            CancellationToken token);
    }
}
