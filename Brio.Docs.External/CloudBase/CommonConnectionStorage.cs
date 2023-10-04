using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.External.CloudBase.Synchronizers;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.External.CloudBase
{
    public class CommonConnectionStorage : IConnectionStorage
    {
        private ICloudManager cloudManager;

        public CommonConnectionStorage(ICloudManager cloudManager)
            =>  this.cloudManager = cloudManager;

        public async Task<bool> DeleteFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDtos, IProgress<double> progress)
        {
            var deletionResult = true;
            int i = 0;
            double itemCount = itemExternalDtos.Count();
            foreach (var item in itemExternalDtos)
            {
                if (!string.IsNullOrWhiteSpace(item?.ExternalID))
                {
                    deletionResult = await cloudManager.DeleteFile(item.ExternalID) && deletionResult;
                    progress?.Report(++i / itemCount);
                }
            }

            return deletionResult;
        }

        public async Task<bool> DownloadFiles(string projectId,
            IEnumerable<ItemExternalDto> itemExternalDtos,
            IProgress<double> progress,
            CancellationToken token)
        {
            int i = 0;
            double itemCount = itemExternalDtos.Count();
            foreach (var item in itemExternalDtos)
            {
                token.ThrowIfCancellationRequested();
                Directory.CreateDirectory(Path.GetDirectoryName(item.FullPath));
                var downloadResult = await cloudManager.PullFile(item.ExternalID, item.FullPath);
                progress?.Report(++i / itemCount);
                if (!downloadResult)
                {
                    progress?.Report(1.0);
                    return false;
                }
            }

            progress?.Report(1.0);
            return true;
        }

        public async Task<IEnumerable<ItemExternalDto>> UploadFiles(
            string projectId,
            IEnumerable<ItemExternalDto> itemExternalDtos,
            IProgress<double> progress)
        {
            var projects =
                await cloudManager.PullAll<ProjectExternalDto>(PathManager.GetTableDir(nameof(ProjectExternalDto)));
            var array = itemExternalDtos.ToArray();
            await ItemsSyncHelper.UploadFiles(
                array,
                cloudManager,
                projects.FirstOrDefault(x => x.ExternalID == projectId)?.Title,
                true);
            progress?.Report(1.0);
            return array;
        }
    }
}
