using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Services
{
    public class ObjectiveTypeService : IObjectiveTypeService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ILogger<ObjectiveTypeService> logger;

        public ObjectiveTypeService(
            DMContext context,
            IMapper mapper,
            ILogger<ObjectiveTypeService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
            logger.LogTrace("ObjectiveTypeService created");
        }

        public async Task<ID<ObjectiveTypeDto>> Add(string typeName)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with typeName: {TypeName}", typeName);
            try
            {
                if (context.ObjectiveTypes.Any(x => x.ConnectionTypeID == null && x.Name == typeName))
                    throw new ArgumentValidationException($"Objective type {typeName} already exists", typeName);
                var objType = mapper.Map<ObjectiveType>(typeName);
                await context.ObjectiveTypes.AddAsync(objType);
                await context.SaveChangesAsync();
                return (ID<ObjectiveTypeDto>)objType.ID;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't add new objective type with typeName = {TypeName}", typeName);
                if (ex is ArgumentValidationException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<ObjectiveTypeDto> Find(ID<ObjectiveTypeDto> id)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started with id: {ID}", id);

            try
            {
                var dbObjectiveType = await context.ObjectiveTypes
                   .FindOrThrowAsync(x => x.ID, (int)id);

                var user = await context.Users.FindOrThrowAsync(CurrentUser.ID);
                var connectionInfo = await context.ConnectionInfos
                    .Where(x => x.User == user)
                    .Include(x => x.ConnectionType)
                    .FirstOrDefaultAsync();
                dbObjectiveType.DefaultDynamicFields = await GetDefaultDynamicFields(connectionInfo, dbObjectiveType.ID);

                logger.LogDebug("Found objective type: {@ObjectiveType}", dbObjectiveType);

                return mapper.Map<ObjectiveTypeDto>(dbObjectiveType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get ObjectiveType {Id}", id);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<ObjectiveTypeDto> Find(string typename)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started with typename: {Typename}", typename);

            try
            {
                var dbObjectiveType = await context.ObjectiveTypes
                   .Include(x => x.DefaultDynamicFields)
                   .FindOrThrowAsync(x => x.Name, typename);

                var user = await context.Users.FindOrThrowAsync(CurrentUser.ID);
                var connectionInfo = await context.ConnectionInfos
                    .Where(x => x.User == user)
                    .Include(x => x.ConnectionType)
                    .FirstOrDefaultAsync();
                dbObjectiveType.DefaultDynamicFields = await GetDefaultDynamicFields(connectionInfo, dbObjectiveType.ID);

                logger.LogDebug("Found objective type: {@ObjectiveType}", dbObjectiveType);

                return mapper.Map<ObjectiveTypeDto>(dbObjectiveType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get ObjectiveType {Typename}", typename);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<ObjectiveTypeDto>> GetObjectiveTypes(ID<UserDto> id)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetObjectiveTypes started with connection type id: {ID}", id);

            try
            {
                var user = await context.Users.FindOrThrowAsync((int)id);
                var connectionInfo = await context.ConnectionInfos
                    .Where(x => x.User == user)
                    .Include(x => x.ConnectionType)
                    .FirstOrDefaultAsync();

                int? connectionTypeId = connectionInfo == null ? (int?)null : (int)connectionInfo.ConnectionTypeID;

                if (connectionTypeId != null)
                    await context.ConnectionTypes.FindOrThrowAsync((int)connectionTypeId);

                var types = await context.ObjectiveTypes.AsNoTracking()
                    .Where(x => x.ConnectionTypeID == connectionTypeId).ToListAsync();

                foreach (var t in types)
                {
                    t.DefaultDynamicFields = await GetDefaultDynamicFields(connectionInfo, t.ID);
                }

                logger.LogDebug("Found objective types: {@ObjectiveTypes}", types);
                return types.Select(x => mapper.Map<ObjectiveTypeDto>(x)).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get list of objective type from connection type {Id}", id);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        private async Task<ICollection<DynamicFieldInfo>> GetDefaultDynamicFields(ConnectionInfo connectionInfo, int typeID)
        {
            var query = context.DynamicFieldInfos
                                 .AsNoTracking()
                                 .Where(x => x.ObjectiveTypeID == typeID);

            if (connectionInfo?.ID != null)
                query = query.Where(x => x.ConnectionInfoID == connectionInfo.ID);
            else
                query = query.Where(x => x.ConnectionInfoID == null);

            return await query.ToListAsync();
        }

        public async Task<bool> Remove(ID<ObjectiveTypeDto> id)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Remove started with id: {ID}", id);
            try
            {
                var type = await context.ObjectiveTypes.FindOrThrowAsync((int)id);
                logger.LogDebug("Found objective type: {@ObjectiveType}", type);
                context.ObjectiveTypes.Remove(type);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't remove objective type with key {ID}", id);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }
    }
}
