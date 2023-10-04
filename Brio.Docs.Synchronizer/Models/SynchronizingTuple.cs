using Brio.Docs.Database;
using Brio.Docs.Synchronization.Interfaces;

namespace Brio.Docs.Synchronization.Models
{
    public class SynchronizingTuple<T> : ISynchronizationChanges
    {
        private T local;
        private T remote;
        private T synchronized;

        public SynchronizingTuple(
                string externalID = null,
                T synchronized = default,
                T local = default,
                T remote = default)
        {
            ExternalID = externalID;
            Synchronized = synchronized;
            Local = local;
            Remote = remote;

            UpdateExternalID();
        }

        public string ExternalID { get; set; }

        public bool HasExternalID => !string.IsNullOrEmpty(ExternalID);

        public T Local
        {
            get => local;
            set
            {
                local = value;
                if (ExternalID == null)
                    UpdateExternalID();
            }
        }

        public T Remote
        {
            get => remote;
            set
            {
                remote = value;
                UpdateExternalID();
            }
        }

        public T Synchronized
        {
            get => synchronized;
            set
            {
                synchronized = value;
                if (ExternalID == null)
                    UpdateExternalID();
            }
        }

        public bool LocalChanged { get; set; }

        public bool RemoteChanged { get; set; }

        public bool SynchronizedChanged { get; set; }

        private void UpdateExternalID()
        {
            if (typeof(T).IsAssignableTo(typeof(ISynchronizableBase)))
            {
                ExternalID ??= ((ISynchronizableBase)Synchronized)?.ExternalID ??
                    ((ISynchronizableBase)Local)?.ExternalID ??
                    ((ISynchronizableBase)Remote)?.ExternalID;
            }
        }
    }
}
