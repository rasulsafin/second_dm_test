using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.External.Utils;

namespace Brio.Docs.External.CloudBase
{
    internal class CachedCloudManager : ICloudManager
    {
        private readonly Dictionary<(Type, string), object> pullAllCache;
        private readonly Dictionary<(Type, string), object> pullByIdCache;
        private readonly Dictionary<(string, string), bool> pullFileCache;

        private readonly Dictionary<string, IEnumerable<CloudElement>> remoteDirectoryFilesCache;

        private readonly ICloudManager wrappedManager;

        public CachedCloudManager(ICloudManager wrappedManager)
        {
            this.wrappedManager = wrappedManager;

            remoteDirectoryFilesCache =
                new Dictionary<string, IEnumerable<CloudElement>>(StringComparer.OrdinalIgnoreCase);
            pullAllCache = new Dictionary<(Type, string), object>(new TupleTypePathComparer());
            pullByIdCache = new Dictionary<(Type, string), object>();
            pullFileCache = new Dictionary<(string, string), bool>(new TuplePathPathComparer());
        }

        public void ClearCache()
        {
            remoteDirectoryFilesCache.Clear();
            pullAllCache.Clear();
            pullFileCache.Clear();
            pullByIdCache.Clear();
        }

        public Task<bool> Delete<T>(string id)
            => throw new NotImplementedException();

        public Task<bool> DeleteFile(string href)
            => throw new NotImplementedException();

        public async Task<IEnumerable<CloudElement>> GetRemoteDirectoryFiles(string directoryPath = "/")
        {
            if (remoteDirectoryFilesCache.TryGetValue(directoryPath, out var result))
                return result;

            result = (await wrappedManager.GetRemoteDirectoryFiles(directoryPath)).ToArray();
            remoteDirectoryFilesCache.Add(directoryPath, result);
            return result;
        }

        public async Task<T> Pull<T>(string id)
        {
            var key = (typeof(T), id);
            if (pullByIdCache.TryGetValue(key, out var result))
                return (T)result;

            result = await wrappedManager.Pull<T>(id);
            pullByIdCache.Add(key, result);
            return (T)result;
        }

        public async Task<List<T>> PullAll<T>(string path)
        {
            var key = (typeof(T), path);
            if (pullAllCache.TryGetValue(key, out var result))
                return (List<T>)result;

            result = await wrappedManager.PullAll<T>(path);
            pullAllCache.Add(key, result);
            return (List<T>)result;
        }

        public async Task<bool> PullFile(string href, string fileName)
        {
            var key = (href, fileName);
            if (pullFileCache.TryGetValue(key, out var result))
                return result;

            result = await wrappedManager.PullFile(href, fileName);
            pullFileCache.Add(key, result);
            return result;
        }

        public Task<bool> Push<T>(T obj, string id)
            => throw new NotImplementedException();

        public Task<string> PushFile(string remoteDirName, string fullPath)
            => throw new NotImplementedException();

        private class TuplePathPathComparer : IEqualityComparer<(string path1, string path2)>
        {
            public bool Equals((string path1, string path2) x, (string path1, string path2) y)
                => StringComparer.OrdinalIgnoreCase.Equals(x.path1, y.path1) &&
                    StringComparer.OrdinalIgnoreCase.Equals(x.path2, y.path2);

            public int GetHashCode((string path1, string path2) obj)
                => HashCode.Combine(
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.path1),
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.path2));
        }

        private class TupleTypePathComparer : IEqualityComparer<(Type type, string path)>
        {
            public bool Equals((Type type, string path) x, (Type type, string path) y)
                => EqualityComparer<Type>.Default.Equals(x.type, y.type) &&
                    StringComparer.OrdinalIgnoreCase.Equals(x.path, y.path);

            public int GetHashCode((Type type, string path) obj)
                => HashCode.Combine(
                    EqualityComparer<Type>.Default.GetHashCode(obj.type),
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.path));
        }
    }
}
