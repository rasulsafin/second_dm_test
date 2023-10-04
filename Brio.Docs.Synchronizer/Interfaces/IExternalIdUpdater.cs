using System.Collections.Generic;

namespace Brio.Docs.Synchronization.Interfaces
{
    /// <summary>
    /// Represents a tool for updating external ids after pushing entities to a remote connection.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    internal interface IExternalIdUpdater<in T>
    {
        /// <summary>
        /// Updates ids for all local entities.
        /// </summary>
        /// <param name="local">Entities to update.</param>
        /// <param name="remote">Entities to get ids.</param>
        void UpdateExternalIds(IEnumerable<T> local, IEnumerable<T> remote);
    }
}
