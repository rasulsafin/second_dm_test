namespace Brio.Docs.Database
{
    /// <summary>
    /// The interface that all synchronizable models must implement.
    /// </summary>
    /// <typeparam name="T">The type of synchronizing model.</typeparam>
    public interface ISynchronizable<T> : ISynchronizableBase
        where T : ISynchronizable<T>
    {
        /// <summary>
        /// Synchronized copy of this model.
        /// </summary>
        public T SynchronizationMate { get; set; }
    }
}
