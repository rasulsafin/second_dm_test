using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Connections.BrioCloud.Synchronization;
using Brio.Docs.External.CloudBase;
using Brio.Docs.External.Extensions;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.BrioCloud
{
    public class BrioCloudConnection : IConnection
    {
        public static readonly string CONNECTION_NAME = "Brio-Cloud";

        private BrioCloudManager manager;

        public BrioCloudConnection()
        {
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token)
        {
            try
            {
                if (await IsAuthDataCorrect(info))
                {
                    await InitiateManager(info);
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

        public async Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            if (manager != null)
            {
                return await manager.GetStatusAsync();
            }

            return new ConnectionStatusDto()
            {
                Status = RemoteConnectionStatus.NeedReconnect,
                Message = "Manager null",
            };
        }

        public Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
        {
            var objectiveType = "BrioCloudIssue";

            var deviationType = new EnumerationTypeExternalDto()
            {
                ExternalID = "Brio.DeviationType",
                Name = "Категория",
                EnumerationValues = new List<EnumerationValueExternalDto>()
                {
                    new EnumerationValueExternalDto()
                    {
                        ExternalID = "Brio.Deviation.DiscrepancyProjectSize",
                        Value = "Несоответствие проектным размерам",
                    },
                    new EnumerationValueExternalDto()
                    {
                        ExternalID = "Brio.Deviation.DisplacementInStructures",
                        Value = "Смещение или отсутствие отверстий в ограждающих конструкциях",
                    },
                    new EnumerationValueExternalDto()
                    {
                        ExternalID = "Brio.Deviation.LackOfElements",
                        Value = "Отсутствие элементов",
                    },
                    new EnumerationValueExternalDto()
                    {
                        ExternalID = "Brio.Deviation.BadLabeling",
                        Value = "Несоответствие проекту по маркировке",
                    },
                    new EnumerationValueExternalDto()
                    {
                        ExternalID = "Brio.Deviation.OffsetFromDesign",
                        Value = "Смещение от проектного положения",
                    },
                    new EnumerationValueExternalDto()
                    {
                        ExternalID = "Brio.Deviation.BadTrace",
                        Value = "Несоответствие трассировки проекту",
                    },
                    new EnumerationValueExternalDto()
                    {
                        ExternalID = "Brio.Deviation.BadInstalation",
                        Value = "Несоответствие места установки проекту",
                    },
                    new EnumerationValueExternalDto()
                    {
                        ExternalID = "Brio.Deviation.Collision",
                        Value = "Коллизия",
                    },
                    new EnumerationValueExternalDto()
                    {
                        ExternalID = "Brio.Deviation.LackOfIsolation",
                        Value = "Отсутствие изоляции",
                    },
                    new EnumerationValueExternalDto()
                    {
                        ExternalID = "Brio.Deviation.Other",
                        Value = "Другое",
                    },
                },
            };

            var criticalStatus = new EnumerationTypeExternalDto()
            {
                ExternalID = "Brio.CriticalStatus",
                Name = "Степень влияния",
                EnumerationValues = new List<EnumerationValueExternalDto>()
                {
                    new EnumerationValueExternalDto()
                    {
                        ExternalID = "Brio.CriticalStatus.Low",
                        Value = "Низкая",
                    },
                    new EnumerationValueExternalDto()
                    {
                        ExternalID = "Brio.CriticalStatus.Normal",
                        Value = "Средняя",
                    },
                    new EnumerationValueExternalDto()
                    {
                        ExternalID = "Brio.CriticalStatus.High",
                        Value = "Высокая",
                    },
                },
            };

            info.EnumerationTypes = new List<EnumerationTypeExternalDto>
            {
                deviationType,
                criticalStatus,
            };

            info.ConnectionType.ObjectiveTypes = new List<ObjectiveTypeExternalDto>
            {
                new ObjectiveTypeExternalDto
                {
                    Name = objectiveType,
                    ExternalId = objectiveType,
                    DefaultDynamicFields = new List<DynamicFieldExternalDto>
                    {
                        new ()
                        {
                            ExternalID = deviationType.ExternalID,
                            Type = DynamicFieldType.ENUM,
                            Name = "Категория",
                            Value = deviationType.EnumerationValues.First().ExternalID,
                        },
                        new ()
                        {
                            ExternalID = criticalStatus.ExternalID,
                            Type = DynamicFieldType.ENUM,
                            Name = "Степень влияния",
                            Value = criticalStatus.EnumerationValues.First().ExternalID,
                        },
                    },
                },
            };

            if (string.IsNullOrWhiteSpace(info.UserExternalID))
                info.UserExternalID = info.GetAuthValue(BrioCloudAuth.USERNAME);

            return Task.FromResult(info);
        }

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
        {
            await InitiateManager(info);
            return new BrioCloudConnectionContext(manager);
        }

        public async Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            await InitiateManager(info);
            return new CommonConnectionStorage(manager);
        }

        private static Task<bool> IsAuthDataCorrect(ConnectionInfoExternalDto info)
        {
            var connect = info.ConnectionType;
            if (connect.Name == CONNECTION_NAME && info.AuthFieldValues.ContainsKey(BrioCloudAuth.USERNAME) && info.AuthFieldValues.ContainsKey(BrioCloudAuth.PASSWORD))
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        private Task InitiateManager(ConnectionInfoExternalDto info)
        {
            string username = info.AuthFieldValues[BrioCloudAuth.USERNAME];
            string password = info.AuthFieldValues[BrioCloudAuth.PASSWORD];

            manager = new BrioCloudManager(new BrioCloudController(username, password));

            return Task.FromResult(true);
        }
    }
}
