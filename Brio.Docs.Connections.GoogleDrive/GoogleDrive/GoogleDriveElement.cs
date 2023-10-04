using Brio.Docs.External.Utils;
using Google.Apis.Drive.v3.Data;

namespace Brio.Docs.Connections.GoogleDrive
{
    public class GoogleDriveElement : CloudElement
    {
        public GoogleDriveElement(File file)
        {
            DisplayName = file.Name;
            Href = file.Id;

            if (file.MimeType != null)
            {
                ContentType = file.MimeType;
                IsDirectory = file.MimeType.Contains("folder");
            }

            if (file.Size.HasValue)
                ContentLength = (ulong)file.Size;
            if (file.ModifiedTime.HasValue)
                LastModified = file.ModifiedTime.Value;
            if (file.CreatedTime.HasValue)
                CreationDate = file.CreatedTime.Value;
        }

        public string MulcaFileUrl { get; private set; }

        public string MulcaDigestUrl { get; private set; }

        public string ETag { get; private set; }
    }
}
