using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Interfaces
{
    /// <summary>
    /// Represents a search tool for finding existing entities in a database.
    /// </summary>
    /// <typeparam name="T">The type of searching.</typeparam>
    internal interface IAttacher<T>
    {
        /// <summary>
        /// The collection of remote entities.
        /// </summary>
        IReadOnlyCollection<T> RemoteCollection { get; set; }

        /// <summary>
        /// Searches and attaches existing entities to a synchronising tuple.
        /// </summary>
        /// <param name="tuple">The tuple to fill.</param>
        /// <returns>A <see cref="Task"/> The task of the operation.</returns>
        Task AttachExisting(SynchronizingTuple<T> tuple);
    }
}
