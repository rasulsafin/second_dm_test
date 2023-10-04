using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace Brio.Docs.Connections.YandexDisk
{
    public static class YandexHelper
    {
        private static readonly string URI_API_YANDEX = "https://webdav.yandex.ru/";

        #region Multu Platform Open Browser
        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
        #endregion

        public static HttpWebRequest RequestGetList(string accessToken, string path)
        {
            var url = URI_API_YANDEX + path;
            var request = WebRequest.CreateHttp(url);
            request.Method = "PROPFIND";
            request.Accept = "*/*";
            request.Headers["Depth"] = "1";
            request.Headers["Authorization"] = "OAuth " + accessToken;
            return request;
        }

        public static HttpWebRequest RequestCreateDir(string accessToken, string pathNewDirectory)
        {
            var url = URI_API_YANDEX + pathNewDirectory;
            var request = WebRequest.CreateHttp(url);
            request.Method = "MKCOL";
            request.Accept = "*/*";
            request.Headers["Authorization"] = "OAuth " + accessToken;
            return request;
        }

        public static HttpWebRequest RequestDownloadFile(string accessToken, string path)
        {
            var url = URI_API_YANDEX + path;
            var request = WebRequest.CreateHttp(url);
            request.Host = "webdav.yandex.ru";
            request.Method = "GET";
            request.Accept = "*/*";
            request.Headers["Authorization"] = "OAuth " + accessToken;
            return request;
        }

        public static HttpWebRequest RequestLoadFile(string accessToken, string path)
        {
            var url = URI_API_YANDEX + path;
            var request = WebRequest.CreateHttp(url);
            request.Host = "webdav.yandex.ru";
            request.Method = "PUT";
            request.Accept = "*/*";
            request.Headers["Authorization"] = "OAuth " + accessToken;

            return request;
        }

        public static HttpWebRequest RequestDelete(string accessToken, string path)
        {
            var url = URI_API_YANDEX + path;
            var request = WebRequest.CreateHttp(url);
            request.Host = "webdav.yandex.ru";
            request.Method = "DELETE";
            request.Accept = "*/*";
            request.Headers["Authorization"] = "OAuth " + accessToken;
            return request;
        }

        public static HttpWebRequest RequestMove(string accessToken, string originPath, string movePath, bool overwrite = true)
        {
            var url = URI_API_YANDEX + originPath;
            var request = WebRequest.CreateHttp(url);
            request.Host = "webdav.yandex.ru";
            request.Method = "MOVE";
            request.Accept = "*/*";
            request.Headers["Authorization"] = "OAuth " + accessToken;
            string encodingString = Uri.EscapeDataString(movePath);
            request.Headers["Destination"] = encodingString;
            if (overwrite == false)
                request.Headers["Overwrite"] = "F";
            return request;
        }
    }
}
