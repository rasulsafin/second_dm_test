using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<ConnectionService> logger;
        private readonly IFactory<Type, IConnection> connectionFactory;
        private readonly IFactory<IServiceScope, ConnectionHelper> connectionHelperFactory;
        private readonly IRequestService requestQueue;
        private readonly ConnectionHelper helper;

        public ConnectionService(
            DMContext context,
            IMapper mapper,
            IRequestService requestQueue,
            ConnectionHelper helper,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ConnectionService> logger,
            IFactory<Type, IConnection> connectionFactory,
            IFactory<IServiceScope, ConnectionHelper> connectionHelperFactory)
        {
            this.context = context;
            this.mapper = mapper;
            this.serviceScopeFactory = serviceScopeFactory;
            this.requestQueue = requestQueue;
            this.helper = helper;
            this.logger = logger;
            this.connectionFactory = connectionFactory;
            this.connectionHelperFactory = connectionHelperFactory;

            logger.LogTrace("ConnectionService created");
        }

        public async Task<ID<ConnectionInfoDto>> Add(ConnectionInfoToCreateDto data)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with data = {@Data}", data);
            try
            {
                var connectionInfo = mapper.Map<ConnectionInfo>(data);
                logger.LogTrace("Mapped connection info = {@ConnectionInfo}", connectionInfo);
                await context.ConnectionInfos.AddAsync(connectionInfo);
                var user = await context.Users.FindOrThrowAsync((int)data.UserID);
                logger.LogDebug("User found: {@User}", user);
                user.ConnectionInfo = connectionInfo;
                await context.SaveChangesAsync();

                return mapper.Map<ID<ConnectionInfoDto>>(connectionInfo.ID);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't add new ConnectionInfo {@Data}", data);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<RequestID> Connect(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogInformation("Connect started with userID = {UserID}", userID);
            try
            {
                var id = Guid.NewGuid().ToString();
                var scope = serviceScopeFactory.CreateScope();
                var scopedHelper = connectionHelperFactory.Create(scope);

                Progress<double> progress = new Progress<double>(v => { requestQueue.SetProgress(v, id); });
                var src = new CancellationTokenSource();
                var task = Task.Factory.StartNew(
                    async () =>
                    {
                        try
                        {
                            logger.LogTrace("Connection task started ({ID})", id);
                            var res = await scopedHelper.ConnectToRemote((int)userID, progress, src.Token);
                            logger.LogInformation("Connection end with {@Res}", res);
                            return res;
                        }
                        catch (OperationCanceledException ex)
                        {
                            logger.LogInformation("Connection canceled");
                            return new RequestResult(ex);
                        }
                        finally
                        {
                            scope.Dispose();
                            logger.LogTrace("Scope for Connect disposed");
                        }
                    },
                    TaskCreationOptions.LongRunning);
                requestQueue.AddRequest(id, task.Unwrap(), src);

                return await Task.FromResult(new RequestID(id));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't start the connection process to remote with user id {UserID}", userID);
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<ConnectionInfoDto> Get(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Get started with userID = {UserID}", userID);
            try
            {
                var connectionInfoFromDb = await helper.GetConnectionInfoFromDb((int)userID);
                logger.LogTrace("Connection Info from DB: {@ConnectionInfoFromDb}", connectionInfoFromDb);
                return mapper.Map<ConnectionInfoDto>(connectionInfoFromDb);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get connection info with user id {UserID}", userID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<ConnectionStatusDto> GetRemoteConnectionStatus(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetRemoteConnectionStatus started with userID = {UserID}", userID);
            try
            {
                var connectionInfo = await helper.GetConnectionInfoFromDb((int)userID);
                logger.LogTrace("Connection Info from DB: {@ConnectionInfo}", connectionInfo);
                if (connectionInfo == null)
                    throw new NotFoundException<ConnectionInfo>("user's id", userID.ToString());

                var connection = connectionFactory.Create(ConnectionCreator.GetConnection(connectionInfo.ConnectionType));

                try
                {
                    return await connection.GetStatus(mapper.Map<ConnectionInfoExternalDto>(connectionInfo));
                }
                catch (Exception ex)
                {
                    return new ConnectionStatusDto() { Status = RemoteConnectionStatus.NeedReconnect, Message = ex.Message };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get status info with user id {UserID}", userID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<EnumerationValueDto>> GetEnumerationVariants(ID<UserDto> userID, ID<EnumerationTypeDto> enumerationTypeID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace(
                "GetEnumerationVariants started with userID = {UserID}, enumerationTypeID = {EnumerationTypeID}",
                userID,
                enumerationTypeID);
            try
            {
                var connectionInfo = await helper.GetConnectionInfoFromDb((int)userID);
                logger.LogTrace("Connection Info from DB: {@ConnectionInfo}", connectionInfo);
                if (connectionInfo == null)
                    throw new NotFoundException<ConnectionInfo>("user's id", userID.ToString());

                var list = connectionInfo.EnumerationValues
                    .Where(x => x.EnumerationValue.EnumerationTypeID == (int)enumerationTypeID)?
                    .Select(x => mapper.Map<EnumerationValueDto>(x.EnumerationValue));
                logger.LogDebug("Enumeration values (id = {EnumerationTypeID}): {@List}", enumerationTypeID, list);
                return list;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get Enumeration Variants with user id {UserID} and enumeration type id {EnumerationTypeID}", userID, enumerationTypeID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }
    }
}
