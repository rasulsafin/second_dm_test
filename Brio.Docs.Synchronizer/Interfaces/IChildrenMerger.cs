using System.Threading.Tasks;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Interfaces
{
    /// <summary>
    /// Represents a merge tool for merging all typed children in a tuple.
    /// </summary>
    /// <typeparam name="TParent">The merging type.</typeparam>
    /// <typeparam name="TChild">The children type.</typeparam>
    // ReSharper disable once UnusedTypeParameter
    internal interface IChildrenMerger<TParent, TChild>
    {
        /// <summary>
        /// Merges all children for the tuple.
        /// </summary>
        /// <param name="tuple">The merging parent tuple.</param>
        /// <returns>The task of the operation.</returns>
        public ValueTask MergeChildren(SynchronizingTuple<TParent> tuple);
    }
}
