using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Connections.YandexDisk.Synchronization;
using Brio.Docs.External.CloudBase;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.YandexDisk
{
    public class YandexConnection : IConnection
    {
        private const string AUTH_FIELD_KEY_TOKEN = "token";
        private const string NAME_CONNECTION = "Yandex Disk";
        private static YandexManager manager;

        public YandexConnection()
        {
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token)
        {
            try
            {
                if (await IsAuthDataCorrect(info))
                {
                    YandexDiskAuth auth = new YandexDiskAuth();
                    if (info.AuthFieldValues == null)
                        info.AuthFieldValues = new Dictionary<string, string>();

                    if (!info.AuthFieldValues.ContainsKey(AUTH_FIELD_KEY_TOKEN))
                    {
                        var tokenNew = await auth.GetYandexDiskToken(info);
                        info.AuthFieldValues.Add(AUTH_FIELD_KEY_TOKEN, tokenNew);
                    }

                    InitiateManager(info);
                }

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

        public Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
        {
            var objectiveType = "YandexDiskIssue";
            info.ConnectionType.ObjectiveTypes = new List<ObjectiveTypeExternalDto>
            {
                new ObjectiveTypeExternalDto { Name = objectiveType, ExternalId = objectiveType },
            };

            if (string.IsNullOrWhiteSpace(info.UserExternalID))
                info.UserExternalID = Guid.NewGuid().ToString();

            return Task.FromResult(info);
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
            if (connect.Name == NAME_CONNECTION)
            {
                if (connect.AppProperties.ContainsKey(YandexDiskAuth.KEY_CLIENT_ID) &&
                    connect.AppProperties.ContainsKey(YandexDiskAuth.KEY_RETURN_URL))
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
        {
            await InitiateManagerForSynchronization(info);
            return YandexConnectionContext.CreateContext(manager);
        }

        public async Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            await InitiateManagerForSynchronization(info);
            return new CommonConnectionStorage(manager);
        }

        private async Task InitiateManagerForSynchronization(ConnectionInfoExternalDto info)
        {
            if (info.AuthFieldValues == null || !info.AuthFieldValues.ContainsKey(AUTH_FIELD_KEY_TOKEN))
            {
                await Connect(info, default);
                return;
            }

            InitiateManager(info);
        }

        private void InitiateManager(ConnectionInfoExternalDto info)
        {
            var token = info.AuthFieldValues[AUTH_FIELD_KEY_TOKEN];
            manager = new YandexManager(new YandexDiskController(token));
        }
    }
}
