using System;

namespace Brio.Docs.Database
{
    /// <summary>
    /// The interface that all synchronizable models must implement.
    /// </summary>
    /// <typeparam name="T">The type of synchronizing model.</typeparam>
    public interface ISynchronizableBase
    {
        /// <summary>
        /// The ID of the model.
        /// </summary>
        int ID { get; set; }

        /// <summary>
        /// The ID of the model in a external system.
        /// </summary>
        string ExternalID { get; set; }

        /// <summary>
        /// The date of a last update of the model.
        /// </summary>
        DateTime UpdatedAt { get; set; }

        /// <summary>
        /// If TRUE model is synchronized, if FAlSE model is unsynchronized.
        /// </summary>
        public bool IsSynchronized { get; set; }

        /// <summary>
        /// The ID of synchronized copy of this model.
        /// </summary>
        public int? SynchronizationMateID { get; set; }
    }
}
