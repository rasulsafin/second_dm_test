namespace Brio.Docs.Synchronization.Models
{
    internal enum SynchronizingAction
    {
        Nothing,
        Merge,
        AddToLocal,
        AddToRemote,
        RemoveFromLocal,
        RemoveFromRemote,
    }
}
