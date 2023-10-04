using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Connections.GoogleDrive.Synchronization;
using Brio.Docs.External.CloudBase;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.GoogleDrive
{
    public class GoogleConnection : IConnection
    {
        private const string NAME_CONNECT = "Google Drive";
        private static GoogleDriveManager manager;
        private ConnectionInfoExternalDto connectionInfo;

        public GoogleConnection()
        {
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token)
        {
            try
            {
                connectionInfo = info;
                GoogleDriveController driveController = new GoogleDriveController();
                await driveController.InitializationAsync(connectionInfo);
                manager = new GoogleDriveManager(driveController);

                return await GetStatus(info);
            }
            catch (Exception ex)
            {
                return new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatus.Error,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            if (manager != null)
            {
                // TODO: make it the proper way.
                return await manager.GetStatusAsync();
            }

            return new ConnectionStatusDto()
            {
                Status = RemoteConnectionStatus.NeedReconnect,
                Message = "Manager null",
            };
        }

        public Task<bool> IsAuthDataCorrect(ConnectionInfoExternalDto info)
        {
            var connect = info.ConnectionType;
            if (connect.Name == NAME_CONNECT)
            {
                if (connect.AppProperties.ContainsKey(GoogleDriveController.APPLICATION_NAME) &&
                    connect.AppProperties.ContainsKey(GoogleDriveController.CLIENT_ID) &&
                    connect.AppProperties.ContainsKey(GoogleDriveController.CLIENT_SECRET))
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        public Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
        {
            info.AuthFieldValues = connectionInfo.AuthFieldValues;
            var objectiveType = "GoogleDriveIssue";
            info.ConnectionType.ObjectiveTypes = new List<ObjectiveTypeExternalDto>
            {
                new ObjectiveTypeExternalDto { Name = objectiveType, ExternalId = objectiveType },
            };

            if (string.IsNullOrWhiteSpace(info.UserExternalID))
                info.UserExternalID = Guid.NewGuid().ToString();

            return Task.FromResult(info);
        }

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
        {
            var connectResult = await Connect(info, default);
            if (connectResult.Status != RemoteConnectionStatus.OK || manager == null)
                return null;

            return GoogleDriveConnectionContext.CreateContext(manager);
        }

        public async Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            await Connect(info, default);
            return new CommonConnectionStorage(manager);
        }
    }
}
