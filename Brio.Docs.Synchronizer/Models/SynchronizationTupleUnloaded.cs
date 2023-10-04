namespace Brio.Docs.Synchronization.Models
{
    internal class SynchronizationTupleUnloaded<T>
    {
        public int? LocalId { get; init; }

        public int? SynchronizedId { get; init; }

        public T Remote { get; init; }
    }
}
