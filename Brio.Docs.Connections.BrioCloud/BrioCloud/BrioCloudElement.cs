using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using Brio.Docs.External.Utils;
using WebDav;

namespace Brio.Docs.Connections.BrioCloud
{
    public class BrioCloudElement : CloudElement
    {
        public string ETag { get; private set; }

        public static List<CloudElement> GetElements(IReadOnlyCollection<WebDavResource> collection, string uri)
        {
            var result = new List<CloudElement>();
            foreach (var element in collection)
            {
                if (Uri.UnescapeDataString(element.Uri) != uri)
                {
                    BrioCloudElement item = GetElement(element);
                    result.Add(item);
                }
            }

            return result;
        }

        private static BrioCloudElement GetElement(WebDavResource element)
        {
            var result = new BrioCloudElement
            {
                Href = GetGenericHref(Uri.UnescapeDataString(element.Uri)),
                IsDirectory = element.IsCollection,
                DisplayName = Path.GetFileName(HttpUtility.UrlDecode(element.Uri.TrimEnd('/'))),
                ContentType = element.ContentType,
                ETag = element.ETag,
            };

            result.CreationDate = element.CreationDate?.ToUniversalTime() ?? default;
            result.ContentLength = (ulong?)element.ContentLength ?? default;
            result.LastModified = element.LastModifiedDate?.ToUniversalTime() ?? default;

            return result;
        }

        private static string GetGenericHref(string href)
        {
            // removing '/remote.php/dav/files/username'
            var regex = new Regex("^/remote[.]php/dav/files/.*?/");
            return regex.Replace(href, "/");
        }
    }
}
