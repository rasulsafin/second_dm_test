using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Brio.Docs.External;
using Brio.Docs.External.Utils;
using WebDav;

namespace Brio.Docs.Connections.BrioCloud
{
    public class BrioCloudController : IDisposable
    {
        private const string BASE_URI = "https://cloud.briogroup.ru";

        private IWebDavClient client;
        private string username;

        public BrioCloudController(string username, string password)
        {
            this.username = username;
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));

            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(BASE_URI),
            };

            httpClient.DefaultRequestHeaders.Add("OCS-APIRequest", "true");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encoded);

            client = new WebDavClient(httpClient);
        }

        private string RootPath
        {
            get
            {
                return $"/remote.php/dav/files/{username}";
            }
        }

        public async Task<IEnumerable<CloudElement>> GetListAsync(string path = "/")
        {
            path = NormalizePath(path);

            var response = await client.Propfind(path);

            if (response.StatusCode == 401)
            {
                throw new UnauthorizedAccessException(response.Description);
            }
            else if (!response.IsSuccessful)
            {
                throw new FileNotFoundException(response.Description);
            }

            var items = BrioCloudElement.GetElements(response.Resources, path);

            return items;
        }

        public async Task<bool> CheckConnectionAsync()
        {
            var response = await client.Propfind(RootPath);

            return response.IsSuccessful;
        }

        public async Task<bool> DownloadFileAsync(string href, string saveFilePath)
        {
            href = NormalizePath(href);

            var result = await client.Propfind(href);

            if (!result.IsSuccessful)
            {
                throw new FileNotFoundException();
            }

            using (var response = await client.GetRawFile(href))
            {
                if (!response.IsSuccessful)
                {
                    throw new WebException(response.Description);
                }

                await using var writer = File.OpenWrite(saveFilePath);
                await response.Stream.CopyToAsync(writer);

                return true;
            }
        }

        public async Task<string> UploadFileAsync(string directoryHref, string filePath)
        {
            var normalizedDirectoryHref = NormalizePath(directoryHref);

            var fileInfo = new FileInfo(filePath);
            string cloudName = PathManager.FileName(normalizedDirectoryHref, fileInfo.Name);

            using (var reader = fileInfo.OpenRead())
            {
                var response = await client.PutFile(cloudName, reader);

                if (response.IsSuccessful)
                {
                    var rawName = PathManager.FileName(directoryHref, fileInfo.Name);
                    return rawName;
                }
                else
                {
                    throw new WebException(response.Description);
                }
            }
        }

        public async Task<bool> DeleteAsync(string href)
        {
            href = NormalizePath(href);

            var response = await client.Delete(href);

            return response.IsSuccessful;
        }

        public async Task<string> GetContentAsync(string href)
        {
            href = NormalizePath(href);

            var result = await client.Propfind(href);

            if (!result.IsSuccessful)
            {
                throw new FileNotFoundException();
            }

            using (var response = await client.GetRawFile(href))
            {
                if (!response.IsSuccessful)
                {
                    throw new WebException(response.Description);
                }

                using var sr = new StreamReader(response.Stream, Encoding.UTF8);
                return await sr.ReadToEndAsync();
            }
        }

        public async Task<bool> SetContentAsync(string path, string content)
        {
            path = NormalizePath(path);

            using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var response = await client.PutFile(path, reader);

                return response.IsSuccessful;
            }
        }

        public async Task<CloudElement> CreateDirAsync(string path, string nameDir)
        {
            path = NormalizePath(path);

            string newPath = PathManager.DirectoryName(path, nameDir);
            var response = await client.Mkcol(newPath);

            if (response.IsSuccessful)
            {
                return new BrioCloudElement();
            }
            else
            {
                throw new WebException(response.Description);
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }

        private string NormalizePath(string path)
        {
            if (!path.StartsWith(RootPath))
            {
                path = RootPath + path;
            }

            return path;
        }
    }
}
