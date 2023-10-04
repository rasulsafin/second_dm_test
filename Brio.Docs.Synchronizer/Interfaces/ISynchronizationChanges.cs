namespace Brio.Docs.Synchronization.Interfaces
{
    /// <summary>
    /// Represents changes of the same model for different databases.
    /// </summary>
    internal interface ISynchronizationChanges
    {
        /// <summary>
        /// True if there are changes in a local model that need to be saved.
        /// </summary>
        bool LocalChanged { get; set; }

        /// <summary>
        /// True if there are changes in a synchronized model that need to be saved.
        /// </summary>
        bool SynchronizedChanged { get; set; }

        /// <summary>
        /// True if there are changes in a remote model that need to be saved.
        /// </summary>
        bool RemoteChanged { get; set; }
    }
}
