using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.External;
using Brio.Docs.External.Utils;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.GoogleDrive
{
    public class GoogleDriveManager : ICloudManager
    {
        private GoogleDriveController controller;
        private bool checkDirApp;
        private Dictionary<string, string> tables = new Dictionary<string, string>();
        private Dictionary<(string parent, string name), string> directories = new Dictionary<(string parent, string name), string>();

        public GoogleDriveManager(GoogleDriveController driveController)
        {
            controller = driveController;
        }

        public string RootDirectoryHref { get; private set; }

        public string TableFolderHref { get; private set; }

        public async Task<ConnectionStatusDto> GetStatusAsync()
        {
            try
            {
                var list = await controller.GetListAsync(RootDirectoryHref);
                if (list != null)
                {
                    return new ConnectionStatusDto()
                    {
                        Status = RemoteConnectionStatus.OK,
                        Message = "Good",
                    };
                }
            }
            catch (Exception ex)
            {
                return new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatus.Error,
                    Message = ex.Message,
                };
            }

            return new ConnectionStatusDto()
            {
                Status = RemoteConnectionStatus.NeedReconnect,
                Message = "Not connect",
            };
        }

        public async Task<bool> Push<T>(T @object, string id)
        {
            try
            {
                var tableHref = await GetTableHref<T>();
                string name = string.Format(PathManager.RECORDED_FILE_FORMAT, id);
                string json = JsonConvert.SerializeObject(@object);
                return await controller.SetContentAsync(json, tableHref, name);
            }
            catch (Exception)
            {
            }

            return false;
        }

        public async Task<T> Pull<T>(string name)
        {
            try
            {
                var tableHref = await GetTableHref<T>();
                string nameWithExtension = string.Format(PathManager.RECORDED_FILE_FORMAT, name);
                string json = await controller.GetContentAsync(tableHref, nameWithExtension);
                T @object = JsonConvert.DeserializeObject<T>(json);
                return @object;
            }
            catch (Exception)
            {
            }

            return default;
        }

        public async Task<bool> Delete<T>(string id)
        {
            var tableHref = await GetTableHref<T>();
            string name = string.Format(PathManager.RECORDED_FILE_FORMAT, id);
            var list = await controller.GetListAsync(tableHref);
            var record = list.FirstOrDefault(x => x.DisplayName == name);
            if (record != null)
            {
                return await controller.DeleteAsync(record.Href);
            }

            return false;
        }

        public async Task<List<T>> PullAll<T>(string path)
        {
            var resultCollection = new List<T>();
            try
            {
                var tableHref = await GetTableHref<T>();
                var elements = await GetRemoteDirectoryFilesByKey(tableHref);
                foreach (var item in elements)
                    resultCollection.Add(await Pull<T>(Path.GetFileNameWithoutExtension(item.DisplayName)));
            }
            catch (FileNotFoundException)
            {
            }

            return resultCollection;
        }

        public async Task<string> PushFile(string remoteDirectoryName, string fullPath)
        {
            string dirHref = await GetDirectoryHref(remoteDirectoryName);
            var res = await controller.LoadFileAsync(dirHref, fullPath);
            return res.Href;
        }

        public async Task<bool> DeleteFile(string href)
        {
            return await controller.DeleteAsync(href);
        }

        public async Task<bool> PullFile(string href, string fileName)
        {
            return await controller.DownloadFileAsync(href, fileName);
        }

        public async Task<IEnumerable<CloudElement>> GetRemoteDirectoryFiles(string directoryPath = "/")
        {
            try
            {
                var directoryHref = await GetDirectoryHref(directoryPath);
                return await controller.GetListAsync(directoryHref);
            }
            catch (FileNotFoundException)
            {
                return Enumerable.Empty<CloudElement>();
            }
        }

        private async Task<IEnumerable<CloudElement>> GetRemoteDirectoryFilesByKey(string directoryHref = "/")
        {
            try
            {
                return await controller.GetListAsync(directoryHref);
            }
            catch (FileNotFoundException)
            {
                return Enumerable.Empty<CloudElement>();
            }
        }

        private async Task<string> GetDirectoryHref(string directoryName)
        {
            await CheckApplicationFolders();
            var foldersInPath = directoryName.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string directoryHref = RootDirectoryHref;
            foreach (var pathPart in foldersInPath.Where(f => !f.Equals(PathManager.APPLICATION_ROOT_DIRECTORY_NAME)))
                directoryHref = await CreateDirectoryIfNecessary(pathPart, directoryHref);

            return directoryHref;
        }

        private async Task<string> CreateDirectoryIfNecessary(string directoryPath, string parentHref)
        {
            var folderExists = directories.ContainsKey((parentHref, directoryPath));
            if (!folderExists)
            {
                var list = await controller.GetListAsync(parentHref);

                foreach (CloudElement element in list.Where(
                    x => x.IsDirectory && !directories.ContainsKey((parentHref, x.DisplayName))))
                {
                    directories.Add((parentHref, element.DisplayName), element.Href);
                    if (element.DisplayName == directoryPath)
                        folderExists = true;
                }

                if (!folderExists)
                {
                    var createdDirectory = await controller.CreateDirectoryAsync(parentHref, directoryPath);
                    directories.Add((parentHref, createdDirectory.DisplayName), createdDirectory.Href);
                }
            }

            return directories[(parentHref, directoryPath)];
        }

        private async Task<string> GetTableHref<T>()
        {
            await CheckApplicationFolders();
            string tableName = typeof(T).Name;
            var res = tables.ContainsKey(tableName);
            if (!res)
            {
                tables.Clear();
                var list = await controller.GetListAsync(TableFolderHref);
                foreach (CloudElement element in list)
                {
                    if (element.IsDirectory)
                        tables.Add(element.DisplayName, element.Href);
                    if (element.DisplayName == tableName)
                        res = true;
                }

                if (!res)
                {
                    var dir = await controller.CreateDirectoryAsync(TableFolderHref, tableName);
                    tables.Add(dir.DisplayName, dir.Href);
                }
            }

            return tables[tableName];
        }

        private async Task CheckApplicationFolders()
        {
            if (!string.IsNullOrWhiteSpace(RootDirectoryHref))
                return;

            IEnumerable<CloudElement> list = await controller.GetListAsync();
            var dirApp = list.FirstOrDefault(x => x.IsDirectory && x.DisplayName == PathManager.APPLICATION_ROOT_DIRECTORY_NAME);
            if (dirApp == null)
            {
                dirApp = await controller.CreateDirectoryAsync(string.Empty, PathManager.APPLICATION_ROOT_DIRECTORY_NAME);
            }

            RootDirectoryHref = dirApp.Href;
            list = await controller.GetListAsync(RootDirectoryHref);
            var dirTable = list.FirstOrDefault(x => x.IsDirectory && x.DisplayName == PathManager.TABLE_DIRECTORY);
            if (dirTable == null)
            {
                dirTable = await controller.CreateDirectoryAsync(RootDirectoryHref, PathManager.TABLE_DIRECTORY);
            }

            TableFolderHref = dirTable.Href;
        }
    }
}
