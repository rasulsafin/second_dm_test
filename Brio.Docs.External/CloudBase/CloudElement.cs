using System;

namespace Brio.Docs.External.Utils
{
    public class CloudElement
    {
        public string DisplayName { get; protected set; }

        public DateTime LastModified { get; protected set; }

        public ulong ContentLength { get; protected set; }

        public string ContentType { get; protected set; }

        public bool IsDirectory { get; protected set; }

        public string Status { get; protected set; }

        public DateTime CreationDate { get; protected set; }

        public string ResourceType { get; protected set; }

        public string Href { get; protected set; }

        public string FileUrl { get; protected set; }
    }
}
