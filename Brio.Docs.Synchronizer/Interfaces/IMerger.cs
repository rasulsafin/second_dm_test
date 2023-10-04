using System.Threading.Tasks;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Interfaces
{
    /// <summary>
    /// Represents a merge tool for merging all data in a tuple and its children.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    internal interface IMerger<T>
    {
        /// <summary>
        /// Merges all data for the tuple.
        /// </summary>
        /// <param name="tuple">The tuple to synchronize.</param>
        /// <returns>The task of the operation.</returns>
        ValueTask Merge(SynchronizingTuple<T> tuple);
    }
}
