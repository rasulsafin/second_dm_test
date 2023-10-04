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
using Brio.Docs.Integration;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Services
{
    public class ConnectionTypeService : IConnectionTypeService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ILogger<ConnectionTypeService> logger;

        public ConnectionTypeService(DMContext context, IMapper mapper, ILogger<ConnectionTypeService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
            logger.LogTrace("ConnectionTypeService created");
        }

        public async Task<ID<ConnectionTypeDto>> Add(string typeName)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with typeName = {@TypeName}", typeName);
            try
            {
                var type = await context.ConnectionTypes.FirstOrDefaultAsync(x => x.Name == typeName);
                logger.LogDebug("Found type: {@ConnectionType}", type);

                if (type != null)
                    throw new ArgumentValidationException("This type name is already being used");

                var connectionType = new ConnectionType { Name = typeName };
                await context.ConnectionTypes.AddAsync(connectionType);
                await context.SaveChangesAsync();
                return (ID<ConnectionTypeDto>)connectionType.ID;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't add new objective type with typeName = {TypeName}", typeName);
                if (ex is ArgumentValidationException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<ConnectionTypeDto> Find(ID<ConnectionTypeDto> id)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started with id = {ID}", id);
            try
            {
                var dbConnectionType = await context.ConnectionTypes
                   .Include(x => x.AppProperties)
                   .Include(x => x.AuthFieldNames)
                   .FindOrThrowAsync(x => x.ID, (int)id);
                logger.LogDebug("Found connection type : {@DBConnectionType}", dbConnectionType);

                return mapper.Map<ConnectionTypeDto>(dbConnectionType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get ConnectionType {Id}", id);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<ConnectionTypeDto> Find(string name)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started with name = {Name}", name);
            try
            {
                var dbConnectionType = await context.ConnectionTypes
                   .Include(x => x.AppProperties)
                   .Include(x => x.AuthFieldNames)
                   .FindOrThrowAsync(x => x.Name, name);
                logger.LogDebug("Found connection type : {@DBConnectionType}", dbConnectionType);
                return mapper.Map<ConnectionTypeDto>(dbConnectionType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get ConnectionType {Name}", name);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<ConnectionTypeDto>> GetAllConnectionTypes()
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetAllConnectionTypes started");
            try
            {
                var dbList = await context.ConnectionTypes
                    .Include(x => x.AppProperties)
                    .Include(x => x.AuthFieldNames)
                    .ToListAsync();
                logger.LogDebug("Found connection types : {@DBList}", dbList);
                return dbList.Select(t => mapper.Map<ConnectionTypeDto>(t)).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get list of all registered connection types");
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<bool> RegisterAll()
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("RegisterAll started");
            try
            {
                var listOfTypes = ConnectionCreator.GetAllConnectionTypes();
                logger.LogDebug("Creator returns: {@ListOfTypes}", listOfTypes);

                foreach (var typeDto in listOfTypes)
                {
                    logger.LogTrace("Registration for {Name}", typeDto.Name);
                    var type = await context.ConnectionTypes
                       .Include(x => x.AppProperties)
                       .Include(x => x.ObjectiveTypes)
                       .Include(x => x.AuthFieldNames)
                       .FirstOrDefaultAsync(x => x.Name == typeDto.Name);
                    logger.LogDebug("Found connection type: {@Type}", type);
                    var update = type != null;

                    if (update)
                    {
                        var properties = type.AppProperties.ToDictionary(x => x.Key, x => x.ID);
                        var authFieldNames = type.AuthFieldNames.ToDictionary(x => x.Name, x => x.ID);

                        type = mapper.Map(typeDto, type);

                        foreach (var property in type.AppProperties)
                        {
                            property.ID = properties.TryGetValue(property.Key, out var value) ? value : 0;
                            property.ConnectionTypeID = type.ID;
                        }

                        foreach (var authFieldName in type.AuthFieldNames)
                        {
                            authFieldName.ID = authFieldNames.TryGetValue(authFieldName.Name, out var value)
                                ? value
                                : 0;
                            authFieldName.ConnectionTypeID = type.ID;
                        }

                        logger.LogDebug("Updating info for connection type: {@Type}", type);
                        context.ConnectionTypes.Update(type);
                    }
                    else
                    {
                        type = mapper.Map<ConnectionType>(typeDto);
                        logger.LogDebug("Adding info for connection type: {@Type}", type);
                        await context.ConnectionTypes.AddAsync(type);
                    }

                    await context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Something went wrong with presented ConnectionTypes");
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<bool> Remove(ID<ConnectionTypeDto> id)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Remove started with id = {ID}", id);
            try
            {
                var type = await context.ConnectionTypes.FindOrThrowAsync((int)id);
                logger.LogDebug("Found type: {@Type}", type);
                context.ConnectionTypes.Remove(type);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't remove connection type with key {ID}", id);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }
    }
}
