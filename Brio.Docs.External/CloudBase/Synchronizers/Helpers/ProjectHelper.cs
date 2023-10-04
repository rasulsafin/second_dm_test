using System;
using System.Linq;
using System.Threading.Tasks;

namespace Brio.Docs.External.CloudBase.Synchronizers
{
    internal class ProjectHelper
    {
        internal static async Task<DateTime> GetItemsDirectoryUpdatedTime(string projectName, ICloudManager manager)
        {
            var folderPath = PathManager.PROJECT_FILES_DIRECTORY;
            folderPath = PathManager.GetNestedDirectory(folderPath);
            var remoteProjectFiles = await manager.GetRemoteDirectoryFiles(folderPath);

            var folderInfo = remoteProjectFiles.Where(x => x.IsDirectory)
                .FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.DisplayName, projectName));
            if (folderInfo == null)
                return default;

            return folderInfo.CreationDate > folderInfo.LastModified
                ? folderInfo.CreationDate
                : folderInfo.LastModified;
        }
    }
}
