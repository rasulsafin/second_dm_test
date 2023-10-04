using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility
{
    public class ConnectionHelper
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly IFactory<Type, IConnection> connectionFactory;
        private readonly ILogger<ConnectionHelper> logger;

        public ConnectionHelper(
            DMContext context,
            IMapper mapper,
            IFactory<Type, IConnection> connectionFactory,
            ILogger<ConnectionHelper> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.connectionFactory = connectionFactory;
            this.logger = logger;
            logger.LogTrace("ConnectionHelper created");
        }

        public async Task<RequestResult> ConnectToRemote(int userID, IProgress<double> progress, CancellationToken token)
        {
            logger.LogTrace("ConnectToRemote started with userID: {@UserID}", userID);
            User user = await context.Users
                            .Include(x => x.ConnectionInfo)
                            .FirstOrDefaultAsync(x => x.ID == userID);
            logger.LogDebug("Found user: {@User}", user);
            if (user == null)
            {
                progress?.Report(1.0);
                return new RequestResult(new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = "Пользователь отсутствует в базе!", });
            }

            token.ThrowIfCancellationRequested();

            // Get connection info from user
            var connectionInfo = await GetConnectionInfoFromDb(user);
            logger.LogDebug("Found connectionInfo: {@ConnectionInfo}", connectionInfo);
            if (connectionInfo == null)
            {
                progress?.Report(1.0);
                return new RequestResult(new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = "Подключение не найдено! (connectionInfo == null)", });
            }

            progress?.Report(0.2);

            var connection = connectionFactory.Create(ConnectionCreator.GetConnection(connectionInfo.ConnectionType));
            var connectionInfoExternalDto = mapper.Map<ConnectionInfoExternalDto>(connectionInfo);
            logger.LogDebug("Mapped connectionInfoExternalDto: {@ConnectionInfo}", connectionInfoExternalDto);

            // Connect to Remote
            var status = new ConnectionStatusDto() { Status = RemoteConnectionStatus.OK };
            token.ThrowIfCancellationRequested();

            try
            {
                logger.LogInformation("External connection started");
                status = await connection.Connect(connectionInfoExternalDto, token);
                logger.LogInformation("External connection finished");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't connect with info {@ConnectionInfo}", connectionInfo);
                progress?.Report(1.0);
                return new RequestResult(new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = e.Message });
            }

            if (status.Status != RemoteConnectionStatus.OK)
                return new RequestResult(status);

            progress?.Report(0.4);

            // Update connection info
            connectionInfoExternalDto = await connection.UpdateConnectionInfo(connectionInfoExternalDto);
            connectionInfo = mapper.Map(connectionInfoExternalDto, connectionInfo);

            user.ExternalID = connectionInfoExternalDto.UserExternalID;

            context.Update(connectionInfo);
            await context.SaveChangesAsync();
            token.ThrowIfCancellationRequested();
            progress?.Report(0.6);

            // Update types stored in connection info
            await UpdateEnumerationObjects(connectionInfo, connectionInfoExternalDto);
            token.ThrowIfCancellationRequested();
            progress?.Report(0.8);

            // Update objective types stored in connection type
            await UpdateObjectiveTypes(connectionInfo, connectionInfoExternalDto);
            progress?.Report(1.0);
            token.ThrowIfCancellationRequested();

            return new RequestResult(status);
        }

        internal async Task<ConnectionInfo> GetConnectionInfoFromDb(int userID)
        {
            logger.LogTrace("GetConnectionInfoFromDb started with userID: {@UserID}", userID);
            User user = await context.Users
               .Include(x => x.ConnectionInfo)
               .FindOrThrowAsync(x => x.ID, userID);
            logger.LogDebug("Found user: {@User}", user);
            return await GetConnectionInfoFromDb(user);
        }

        private async Task<ConnectionInfo> GetConnectionInfoFromDb(User user)
        {
            logger.LogTrace("GetConnectionInfoFromDb started with user: {@User}", user);
            if (user == null)
                return null;

            var info = await context.ConnectionInfos
                .Include(x => x.ConnectionType)
                    .ThenInclude(x => x.AppProperties)
                .Include(x => x.ConnectionType)
                    .ThenInclude(x => x.ObjectiveTypes)
                        .ThenInclude(x => x.DefaultDynamicFields)
                .Include(x => x.ConnectionType)
                    .ThenInclude(x => x.AuthFieldNames)
                .Include(x => x.EnumerationTypes)
                    .ThenInclude(x => x.EnumerationType)
                .Include(x => x.EnumerationValues)
                    .ThenInclude(x => x.EnumerationValue)
                .Include(x => x.AuthFieldValues)
                .FirstOrDefaultAsync(x => x.ID == user.ConnectionInfoID);
            logger.LogDebug("Found info: {@Info}", info);

            return info;
        }

        private async Task<EnumerationType> LinkEnumerationTypes(EnumerationTypeExternalDto enumType, ConnectionInfo connectionInfo)
        {
            logger.LogTrace(
                "LinkEnumerationTypes started with enumType: {@EnumerationType} & connectionInfo: {@ConnectionInfo})",
                enumType,
                connectionInfo);
            var enumTypeDb = await CheckEnumerationTypeToLink(enumType, (int)connectionInfo.ID);
            logger.LogDebug("Found type: {@EnumerationType}", enumTypeDb);
            if (enumTypeDb != null)
            {
                connectionInfo.EnumerationTypes.Add(new ConnectionInfoEnumerationType
                {
                    ConnectionInfoID = connectionInfo.ID,
                    EnumerationTypeID = enumTypeDb.ID,
                });

                await context.SaveChangesAsync();
            }
            else
            {
                enumTypeDb = await context.EnumerationTypes
                    .FirstOrDefaultAsync(i => i.ExternalId == enumType.ExternalID);
            }

            return enumTypeDb;
        }

        private async Task LinkEnumerationValues(EnumerationValueExternalDto enumVal, EnumerationType type, ConnectionInfo connectionInfo)
        {
            logger.LogTrace(
                "LinkEnumerationValues started with enumVal: {@EnumerationValue}), type: {@EnumerationType} & {@ConnectionInfo}",
                enumVal,
                type,
                connectionInfo);
            var enumValueDb = await CheckEnumerationValueToLink(enumVal, type, (int)connectionInfo.ID);
            logger.LogDebug("Found value: {@EnumerationValue}", enumValueDb);
            if (enumValueDb == null)
                return;

            connectionInfo.EnumerationValues.Add(new ConnectionInfoEnumerationValue
            {
                ConnectionInfoID = connectionInfo.ID,
                EnumerationValueID = enumValueDb.ID,
            });

            await context.SaveChangesAsync();
        }

        private async Task<EnumerationType> CheckEnumerationTypeToLink(EnumerationTypeExternalDto enumTypeDto, int connectionInfoID)
        {
            logger.LogTrace(
                "CheckEnumerationTypeToLink started with type: {@EnumerationType} & connectionInfoID: {@ConnectionInfoID}",
                enumTypeDto,
                connectionInfoID);
            var enumTypeDb = await context.EnumerationTypes
                    .FirstOrDefaultAsync(i => i.ExternalId == enumTypeDto.ExternalID);
            logger.LogDebug("Found type: {@EnumerationType}", enumTypeDb);

            if (enumTypeDb == null)
            {
                enumTypeDb = mapper.Map<EnumerationType>(enumTypeDto);
                logger.LogDebug("Mapped type: {@EnumerationType}", enumTypeDb);
                var connectionType = context.ConnectionInfos.Include(x => x.ConnectionType).FirstOrDefault(x => x.ID == connectionInfoID).ConnectionType;
                logger.LogDebug("Found type: {@ConnectionType}", connectionType);
                enumTypeDb.ConnectionType = connectionType;

                await context.EnumerationTypes.AddAsync(enumTypeDb);
                await context.SaveChangesAsync();
                return enumTypeDb;
            }

            bool alreadyLinked = await context.ConnectionInfoEnumerationTypes
                        .AnyAsync(i => i.EnumerationTypeID == enumTypeDb.ID && i.ConnectionInfoID == connectionInfoID);
            logger.LogDebug("Enumeration type is already linked: {IsLinked}", alreadyLinked);

            if (alreadyLinked)
                return null;

            return enumTypeDb;
        }

        private async Task<EnumerationValue> CheckEnumerationValueToLink(EnumerationValueExternalDto enumValueDto, EnumerationType type, int connectionInfoID)
        {
            logger.LogTrace(
                "CheckEnumerationValueToLink started with enumValueDto: {@User}, type: {@EnumerationType}, connectionInfoID {@ConnectionInfoID}",
                enumValueDto,
                type,
                connectionInfoID);
            var enumValueDb = await context.EnumerationValues
                    .FirstOrDefaultAsync(i => i.ExternalId == enumValueDto.ExternalID);
            logger.LogDebug("Found value: {@EnumerationValue}", enumValueDb);

            if (enumValueDb == null)
            {
                enumValueDb = mapper.Map<EnumerationValue>(enumValueDto);
                logger.LogDebug("Mapped value: {@EnumerationValue}", enumValueDb);
                enumValueDb.EnumerationType = type;
                await context.EnumerationValues.AddAsync(enumValueDb);
                await context.SaveChangesAsync();
                return enumValueDb;
            }

            bool alreadyLinked = await context.ConnectionInfoEnumerationValues
                        .AnyAsync(i => i.EnumerationValueID == enumValueDb.ID && i.ConnectionInfoID == connectionInfoID);
            logger.LogDebug("Enumeration value is already linked: {IsLinked}", alreadyLinked);

            if (alreadyLinked)
                return null;

            return enumValueDb;
        }

        private async Task UpdateObjectiveTypes(ConnectionInfo connectionInfo, ConnectionInfoExternalDto connectionInfoExternalDto)
        {
            foreach (var externalType in connectionInfoExternalDto.ConnectionType.ObjectiveTypes)
            {
                var dbType = connectionInfo.ConnectionType.ObjectiveTypes.FirstOrDefault(x => x.ExternalId == externalType.ExternalId);
                if (dbType != null)
                {
                    dbType.DefaultDynamicFields = dbType.DefaultDynamicFields.Where(d => d.ConnectionInfoID != connectionInfo.ID).ToList();
                    var newDefaultDynamicFileds = mapper.Map<ICollection<DynamicFieldInfo>>(externalType.DefaultDynamicFields);
                    foreach (var d in newDefaultDynamicFileds)
                    {
                        d.ConnectionInfoID = connectionInfo.ID;
                        d.ConnectionInfo = connectionInfo;
                        dbType.DefaultDynamicFields.Add(d);
                    }

                    dbType.Name = externalType.Name;
                }
                else
                {
                    var newType = mapper.Map<ObjectiveType>(externalType);
                    connectionInfo.ConnectionType.ObjectiveTypes.Add(newType);
                }
            }

            context.Update(connectionInfo);
            await context.SaveChangesAsync();
        }

        private async Task UpdateEnumerationObjects(ConnectionInfo connectionInfo, ConnectionInfoExternalDto connectionInfoExternalDto)
        {
            logger.LogTrace(
                "UpdateEnumerationObjects started with connectionInfo: {@ConnectionInfo}, connectionInfoExternalDto: {@UpdatedConnectionInfo}",
                connectionInfo,
                connectionInfoExternalDto);

            // Update types stored in connection info
            var newTypes = connectionInfoExternalDto.EnumerationTypes ?? Enumerable.Empty<EnumerationTypeExternalDto>();
            var currentEnumerationTypes = connectionInfo.EnumerationTypes.ToList();
            var typesToRemove = currentEnumerationTypes?
                .Where(x => newTypes.All(t => t.ExternalID != x.EnumerationType.ExternalId))
                .ToList();
            logger.LogDebug("Types to remove: {@Links}", typesToRemove);
            context.ConnectionInfoEnumerationTypes.RemoveRange(typesToRemove);

            // Update values stored in connection info
            var newValues = connectionInfoExternalDto.EnumerationTypes?
                .SelectMany(x => x.EnumerationValues)?.ToList() ?? Enumerable.Empty<EnumerationValueExternalDto>();
            var currentEnumerationValues = connectionInfo.EnumerationValues.ToList();
            var valuesToRemove = currentEnumerationValues?
                .Where(x => newValues.All(t => t.ExternalID != x.EnumerationValue.ExternalId))
                .ToList();
            context.ConnectionInfoEnumerationValues.RemoveRange(valuesToRemove);

            foreach (var enumType in newTypes)
            {
                var linkedType = await LinkEnumerationTypes(enumType, connectionInfo);
                foreach (var enumVal in enumType.EnumerationValues)
                {
                    await LinkEnumerationValues(enumVal, linkedType, connectionInfo);
                }
            }

            context.Update(connectionInfo);
            await context.SaveChangesAsync();
        }
    }
}
