using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.External.Utils;
using Brio.Docs.Integration.Dtos;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;

namespace Brio.Docs.Connections.GoogleDrive
{
    public class GoogleDriveController : IDisposable
    {
        // https://developers.google.com/drive/api/v3/search-files?hl=ru
        public static readonly string CLIENT_ID = "CLIENT_ID";
        public static readonly string CLIENT_SECRET = "CLIENT_SECRET";
        public static readonly string APPLICATION_NAME = "APPLICATION_NAME";
        public static readonly string USER_AUTH_FIELD_NAME = "user";

        public static readonly string[] SCOPES = { DriveService.Scope.Drive };
        private static readonly string REQUEST_FIELDS = "nextPageToken, files(id, name, size, mimeType, modifiedTime, createdTime)";
        private static readonly string MIME_TYPE_FOLDER = "application/vnd.google-apps.folder";

        private UserCredential credential;
        private DriveService service;

        public GoogleDriveController()
        {
        }

        public async Task InitializationAsync(ConnectionInfoExternalDto info)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            ClientSecrets clientSecrets = new ClientSecrets
            {
                ClientId = info.ConnectionType.AppProperties[CLIENT_ID],
                ClientSecret = info.ConnectionType.AppProperties[CLIENT_SECRET],
            };

            DataStore dataStore = new DataStore(info);

            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets: clientSecrets,
                    scopes: SCOPES,
                    user: USER_AUTH_FIELD_NAME,
                    taskCancellationToken: cancellationTokenSource.Token,
                    dataStore: dataStore);

            service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = Properties.Resources.ApplicationName,
            });
        }

        #region PROPFIND

        public async Task<IEnumerable<CloudElement>> GetListAsync(string id = "")
        {
            var result = new List<Google.Apis.Drive.v3.Data.File>();
            var nextPageToken = string.Empty;

            do
            {
                var request = service.Files.List();
                request.Fields = REQUEST_FIELDS;
                request.PageSize = 100;
                request.Spaces = "Drive";
                if (!string.IsNullOrWhiteSpace(id))
                    request.Q = $"'{id}' in parents";
                else
                    request.Q = $"'root' in parents";
                if (!string.IsNullOrEmpty(nextPageToken))
                    request.PageToken = nextPageToken;

                // Exclude trashed files
                request.Q = $"{request.Q} and trashed = false";

                try
                {
                    var fileList = await request.ExecuteAsync();
                    result.AddRange(fileList.Files);
                    nextPageToken = fileList.NextPageToken;
                }
                catch (GoogleApiException ge)
                {
                    if (ge.Error.Code == 404)
                        throw new FileNotFoundException();

                    throw;
                }
            }
            while (!string.IsNullOrEmpty(nextPageToken));

            List<GoogleDriveElement> elements = new List<GoogleDriveElement>();
            foreach (var item in result)
            {
                var element = new GoogleDriveElement(item);
                elements.Add(element);
            }

            return elements;
        }

        public async Task<IEnumerable<CloudElement>> GetListAsync(string parentId, string partOfName)
        {
            var result = new List<Google.Apis.Drive.v3.Data.File>();
            var nextPageToken = string.Empty;

            do
            {
                var request = service.Files.List();
                request.Fields = REQUEST_FIELDS;
                request.PageSize = 100;
                request.Spaces = "Drive";

                var q = new List<string>();
                q.Add($"'{parentId}' in parents");
                q.Add($"name contains '{partOfName}'");
                request.Q = q.Aggregate((a, b) => $"{a} and {b}");

                if (!string.IsNullOrEmpty(nextPageToken))
                    request.PageToken = nextPageToken;

                var fileList = await request.ExecuteAsync();

                result.AddRange(fileList.Files);
                nextPageToken = fileList.NextPageToken;
            }
            while (!string.IsNullOrEmpty(nextPageToken));

            List<GoogleDriveElement> elements = new List<GoogleDriveElement>();
            foreach (Google.Apis.Drive.v3.Data.File item in result)
            {
                var element = new GoogleDriveElement(item);
                elements.Add(element);
            }

            return elements;
        }

        public async Task<CloudElement> GetInfoAsync(string id)
        {
            try
            {
                var request = service.Files.Get(id);
                request.Fields = "*";
                var file = await request.ExecuteAsync();
                if (file != null)
                    return new GoogleDriveElement(file);
            }
            catch
            {
            }

            return null;
        }

        #endregion
        #region Create Directory
        public async Task<CloudElement> CreateDirectoryAsync(string idParent, string nameDir)
        {
            var fileDrive = new Google.Apis.Drive.v3.Data.File
            {
                Name = nameDir,
                MimeType = MIME_TYPE_FOLDER,
            };
            if (!string.IsNullOrWhiteSpace(idParent))
                fileDrive.Parents = new List<string>() { idParent };

            var request = service.Files.Create(fileDrive);
            var result = await request.ExecuteAsync();
            if (result != null)
                return new GoogleDriveElement(result);
            return null;
        }

        #endregion
        #region Content
        public async Task<bool> SetContentAsync(string content, string idParent, string name)
        {
            var info = await GetInfoAsync(idParent);
            IUploadProgress result = null;

            if (info.IsDirectory)
            {
                var contentType = (string)null;
                var fileDrive = new Google.Apis.Drive.v3.Data.File
                {
                    Name = name,
                };

                var infos = await GetListAsync(idParent);

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                {
                    var element = infos.FirstOrDefault(x => x.DisplayName == name);
                    if (element != null)
                    {
                        var request = service.Files.Update(fileDrive, element.Href, stream, contentType);
                        result = await request.UploadAsync();
                    }
                    else
                    {
                        fileDrive.Parents = new List<string> { idParent };
                        var request = service.Files.Create(fileDrive, stream, contentType);
                        result = await request.UploadAsync();
                    }
                }
            }

            return result.Exception == null;
        }

        public async Task<string> GetContentAsync(string idParent, string name)
        {
            var info = await GetInfoAsync(idParent);
            IDownloadProgress result = null;

            if (info.IsDirectory)
            {
                var infos = await GetListAsync(idParent);
                var element = infos.FirstOrDefault(x => x.DisplayName == name);
                if (element != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        var href = element.Href;
                        var request = service.Files.Get(href);
                        result = await request.DownloadAsync(stream);
                        var buffer = stream.ToArray();
                        var content = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        if (result.Status == DownloadStatus.Completed)
                            return content;
                    }
                }
            }

            return string.Empty;
        }
        #endregion
        #region Download File

        public async Task<bool> DownloadFileAsync(string href, string currentPath, Action<ulong, ulong> updateProgress = null)
        {
            using (var stream = System.IO.File.Create(currentPath))
            {
                var request = service.Files.Get(href);
                await request.DownloadAsync(stream);
                return true;
            }
        }
        #endregion
        #region Delete file and directory

        public async Task<bool> DeleteAsync(string href)
        {
            var request = service.Files.Delete(href);
            var response = await request.ExecuteAsync();
            return true;
        }
        #endregion
        #region Load File

        public async Task<CloudElement> LoadFileAsync(string idParent, string fileName, Action<ulong, ulong> progressChenge = null)
        {
            var info = await GetInfoAsync(idParent);
            IUploadProgress result = null;

            if (info.IsDirectory)
            {
                FileInfo fileInfo = new FileInfo(fileName);
                var contentType = (string)null;
                var fileDrive = new Google.Apis.Drive.v3.Data.File
                {
                    Name = fileInfo.Name,
                };

                var infos = await GetListAsync(idParent);

                using (var stream = fileInfo.OpenRead())
                {
                    if (infos.Any(x => x.DisplayName == fileInfo.Name))
                    {
                        fileDrive.Id = infos.FirstOrDefault()?.Href;

                        // TODO: Change this
                        // Do not update file for the moment
                        //var request = service.Files.Update(fileDrive, href, stream, contentType);
                        //result = await request.UploadAsync();
                    }
                    else
                    {
                        fileDrive.Parents = new List<string> { idParent };
                        var request = service.Files.Create(fileDrive, stream, contentType);
                        result = await request.UploadAsync();
                        var updatedInfos = await GetListAsync(idParent);
                        fileDrive.Id = updatedInfos.FirstOrDefault(i => i.DisplayName == fileInfo.Name)?.Href;
                    }
                }

                return new GoogleDriveElement(fileDrive);
            }

            return null;
        }

        public void Dispose()
        {
            ((IDisposable)service).Dispose();
        }
        #endregion
    }
}
