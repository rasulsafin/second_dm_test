using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Brio.Docs.Integration.Interfaces
{
    /// <summary>
    /// Represents working with document management entities.
    /// </summary>
    /// <typeparam name="T">The type of entities.</typeparam>
    public interface ISynchronizer<T>
    {
        /// <summary>
        /// Add entity to external connection.
        /// </summary>
        /// <param name="obj">Entity to be added.</param>
        /// <returns>The added entity.</returns>
        Task<T> Add(T obj);

        /// <summary>
        /// Remove entity from external connection.
        /// </summary>
        /// <param name="obj">Entity to be removed.</param>
        /// <returns>The removed entity.</returns>
        Task<T> Remove(T obj);

        /// <summary>
        /// Update entity from external connection.
        /// </summary>
        /// <param name="obj">Entity to be updated.</param>
        /// <returns>The updated entity.</returns>
        Task<T> Update(T obj);

        /// <summary>
        /// Gets external identifiers for entities updated after the date.
        /// </summary>
        /// <param name="date">Date of the last synchronization.</param>
        /// <returns>Collection of external ids of changed entities.</returns>
        Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date);

        /// <summary>
        /// Gets entities by ids.
        /// </summary>
        /// <param name="ids">Entity IDs to synchronizing.</param>
        /// <returns>Entities with equals ids.</returns>
        Task<IReadOnlyCollection<T>> Get(IReadOnlyCollection<string> ids);
    }
}
