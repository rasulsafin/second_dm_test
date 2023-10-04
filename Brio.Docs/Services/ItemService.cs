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
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Services
{
    public class ItemService : IItemService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly IFactory<IServiceScope, Type, IConnection> connectionFactory;
        private readonly IFactory<IServiceScope, DMContext> contextFactory;
        private readonly IFactory<IServiceScope, IMapper> mapperFactory;
        private readonly IRequestService requestQueue;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<ItemService> logger;

        public ItemService(
            DMContext context,
            IMapper mapper,
            IFactory<IServiceScope, Type, IConnection> connectionFactory,
            IFactory<IServiceScope, DMContext> contextFactory,
            IFactory<IServiceScope, IMapper> mapperFactory,
            IRequestService requestQueue,
            IServiceScopeFactory scopeFactory,
            ILogger<ItemService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.connectionFactory = connectionFactory;
            this.contextFactory = contextFactory;
            this.mapperFactory = mapperFactory;
            this.requestQueue = requestQueue;
            this.scopeFactory = scopeFactory;
            this.logger = logger;
            logger.LogTrace("ItemService created");
        }

        public async Task<ID<ItemDto>> LinkItem(ID<ProjectDto> projectId, ItemDto itemDto)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("LinkItem started to project: {@Project} and item: {@Item}", projectId, itemDto);

            try
            {
                if (await context.Projects.AsNoTracking().AllAsync(x => x.ID != (int)projectId))
                    throw new NotFoundException<Project>((int)projectId);

                var projectItems = context.Items.Where(
                    x => x.ProjectID == (int)projectId ||
                        x.Objectives.Select(o => o.Objective)
                           .Select(o => o.Project)
                           .FirstOrDefault()
                           .ID == (int)projectId);

                var item = projectItems.FirstOrDefault(
                    x => x.ID == (int)itemDto.ID || x.RelativePath == itemDto.RelativePath);

                logger.LogDebug("Found item: {@Item}", item);
                var isNew = item == null;

                if (isNew)
                {
                    item = mapper.Map<Item>(itemDto);
                    logger.LogDebug("Mapped item: {@Item}", item);
                }

                if (isNew || item.ProjectID != (int)projectId)
                {
                    item.ProjectID = (int)projectId;

                    if (isNew)
                        context.Items.Add(item);
                    else
                        context.Items.Update(item);

                    await context.SaveChangesAsync();
                    logger.LogDebug(
                        "Saved changes after linking item {Item} to project {@Project}",
                        item.ID,
                        projectId);
                }

                return new ID<ItemDto>(item.ID);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't add item {@Item} to project {@Project}", itemDto, projectId);
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<RequestID> DeleteItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("DeleteItems started for user {@UserID} with itemIds: {@ItemIds}", userID, itemIds);
            try
            {
                var ids = itemIds.Select(x => (int)x).ToArray();
                var dbItems = await context.Items
                    .Where(x => ids.Contains(x.ID))
                    .ToListAsync();
                logger.LogDebug("Found items: {@DBItems}", dbItems);

                var projectID = dbItems.FirstOrDefault()?.ProjectID ?? -1;
                var project = await context.Projects
                    .Where(x => x.ID == projectID)
                    .FirstOrDefaultAsync();
                logger.LogDebug("Found project: {@Project}", project);

                var scope = scopeFactory.CreateScope();
                var storage = await GetStorage(scope, userID);

                var id = Guid.NewGuid().ToString();
                Progress<double> progress = new (v => { requestQueue.SetProgress(v, id); });
                var data = dbItems.Select(x => mapper.Map<ItemExternalDto>(x)).ToList();
                var src = new CancellationTokenSource();
                var scopeContext = contextFactory.Create(scope);

                var task = Task.Factory.StartNew(
                    async () =>
                    {
                        try
                        {
                            logger.LogTrace("DeleteItems task started ({ID})", id);
                            var result = await storage.DeleteFiles(project?.ExternalID, data, progress);
                            logger.LogDebug("DeleteItems is successful: {Result}", result);

                            if (result)
                            {
                                foreach (var item in dbItems)
                                {
                                    scopeContext.Items.Remove(item);
                                }

                                await scopeContext.SaveChangesAsync();
                            }

                            return new RequestResult(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Can't delete items {@ItemIds} with user key {UserID}", itemIds, userID);
                            return new RequestResult(false);
                        }
                        finally
                        {
                            scope.Dispose();
                        }
                    },
                    TaskCreationOptions.LongRunning);
                requestQueue.AddRequest(id, task.Unwrap(), src);

                return new RequestID(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't download items {@ItemIds} with user key {UserID}", itemIds, userID);
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<RequestID> DownloadItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("DownloadItems started for user {@UserID} with itemIds: {@ItemIds}", userID, itemIds);
            try
            {
                var ids = itemIds.Select(x => (int)x).ToArray();
                var dbItems = await context.Items
                    .Where(x => ids.Contains(x.ID))
                    .ToListAsync();
                logger.LogDebug("Found items: {@DBItems}", dbItems);

                var projectID = dbItems.FirstOrDefault()?.ProjectID ?? -1;
                var project = await context.Projects
                    .Where(x => x.ID == projectID)
                    .FirstOrDefaultAsync();
                logger.LogDebug("Found project: {@Project}", project);

                var scope = scopeFactory.CreateScope();
                var storage = await GetStorage(scope, userID);

                var id = Guid.NewGuid().ToString();
                Progress<double> progress = new (v => { requestQueue.SetProgress(v, id); });
                var data = dbItems.Select(x => mapper.Map<ItemExternalDto>(x)).ToList();
                var src = new CancellationTokenSource();

                var task = Task.Factory.StartNew(
                    async () =>
                    {
                        try
                        {
                            logger.LogTrace("DownloadItems task started ({ID})", id);
                            var result = await storage.DownloadFiles(project?.ExternalID, data, progress, src.Token);
                            logger.LogDebug("Downloading is successful: {Result}", result);
                            return new RequestResult(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Can't download items {@ItemIds} with user key {UserID}", itemIds, userID);
                            return new RequestResult(false);
                        }
                        finally
                        {
                            scope.Dispose();
                        }
                    },
                    TaskCreationOptions.LongRunning);
                requestQueue.AddRequest(id, task.Unwrap(), src);

                return new RequestID(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't download items {@ItemIds} with user key {UserID}", itemIds, userID);
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<ItemDto> Find(ID<ItemDto> itemID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started with itemID: {@ItemID}", itemID);
            try
            {
                var dbItem = await context.Items.FindOrThrowAsync((int)itemID);
                logger.LogDebug("Found dbItem: {@DbItem}", dbItem);
                return mapper.Map<ItemDto>(dbItem);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get item with key {ItemID}", itemID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID)
        {
            logger.LogTrace("GetItems started with projectID: {@ProjectID}", projectID);
            try
            {
                await context.Projects.FindOrThrowAsync((int)projectID);
                var dbItems = (await context.Projects
                       .Include(x => x.Items)
                       .FirstOrDefaultAsync(x => x.ID == (int)projectID))?.Items
                 ?? Enumerable.Empty<Item>();
                logger.LogDebug("Found dbItems: {@DbItem}", dbItems);
                return dbItems.Select(x => mapper.Map<ItemDto>(x)).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't find items by project key {@ProjectID}", projectID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetItems started with objectiveID: {@ObjectiveID}", objectiveID);
            try
            {
                await context.Objectives.FindOrThrowAsync((int)objectiveID);
                var dbItems = await context.ObjectiveItems
                    .Where(x => x.ObjectiveID == (int)objectiveID)
                    .Select(x => x.Item)
                    .ToListAsync();
                logger.LogDebug("Found dbItems: {@DbItem}", dbItems);
                return dbItems.Select(x => mapper.Map<ItemDto>(x)).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't find items by objective key {@ObjectiveID}", objectiveID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<bool> Update(ItemDto item)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Update started with item: {@Item}", item);
            try
            {
                var dbItem = await context.Items.FindOrThrowAsync((int)item.ID);
                logger.LogDebug("Found dbItem: {@DbItem}", dbItem);

                dbItem.ItemType = (int)item.ItemType;
                dbItem.RelativePath = item.RelativePath;
                context.Items.Update(dbItem);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't update item {@Item}", item);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<RequestID> UploadItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("UploadItems started for user {@UserID} with itemIds: {@ItemIds}", userID, itemIds);
            try
            {
                var ids = itemIds.Select(x => (int)x).ToArray();
                var dbItems = await context.Items
                    .Where(x => ids.Contains(x.ID))
                    .ToListAsync();
                logger.LogDebug("Found items: {@DBItems}", dbItems);

                var projectID = dbItems.FirstOrDefault()?.ProjectID ?? -1;
                var project = await context.Projects
                    .Where(x => x.ID == projectID)
                    .FirstOrDefaultAsync();
                logger.LogDebug("Found project: {@Project}", project);

                var scope = scopeFactory.CreateScope();
                var storage = await GetStorage(scope, userID);
                var scopedMapper = mapperFactory.Create(scope);
                var scopedContext = contextFactory.Create(scope);

                var id = Guid.NewGuid().ToString();
                Progress<double> progress = new (v => { requestQueue.SetProgress(v * 0.9, id); });
                var data = dbItems.Select(x => mapper.Map<ItemExternalDto>(x)).ToList();
                var src = new CancellationTokenSource();

                var task = Task.Factory.StartNew(
                    async () =>
                    {
                        try
                        {
                            logger.LogTrace("UploadItems task started ({ID})", id);
                            var result = await storage.UploadFiles(project?.ExternalID, data, progress);
                            logger.LogDebug("Uploading is successful: {@Result}", result);
                            var updatedItems = result.Select(x => scopedMapper.Map<Item>(x)).ToArray();

                            foreach (var updatedItem in updatedItems.Where(x => !string.IsNullOrEmpty(x.ExternalID)))
                            {
                                var found = scopedContext.Items.FirstOrDefault(x => x.RelativePath == updatedItem.RelativePath);

                                if (found != null)
                                {
                                    found.ExternalID = updatedItem.ExternalID;
                                    scopedContext.Items.Update(found);
                                }
                            }

                            await scopedContext.SaveChangesAsync();
                            var allUploaded = updatedItems.All(x => x.ExternalID != null);
                            return new RequestResult(allUploaded);
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Can't upload items {@ItemIds} with user key {UserID}", itemIds, userID);
                            return new RequestResult(false);
                        }
                        finally
                        {
                            scope.Dispose();
                        }
                    },
                    TaskCreationOptions.LongRunning);
                requestQueue.AddRequest(id, task.Unwrap(), src);

                return new RequestID(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't upload items {@ItemIds} with user key {UserID}", itemIds, userID);
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        private async Task<IConnectionStorage> GetStorage(IServiceScope scope, ID<UserDto> userID)
        {
            var connectionInfo = await context.ConnectionInfos.Include(x => x.AuthFieldValues)
               .FirstOrDefaultAsync(x => x.User.ID == (int)userID);
            logger.LogDebug("Found connection info: {@ConnectionInfo}", connectionInfo);

            if (connectionInfo == null)
                throw new NotFoundException<ConnectionInfo>($"ConnectionInfo for user {userID} not found");

            var connectionType = await context.Users.Where(x => x.ID == (int)userID)
               .Select(x => x.ConnectionInfo.ConnectionType)
               .FirstAsync();

            logger.LogDebug("Found connection type: {@ConnectionType}", connectionType);
            var connection = connectionFactory.Create(scope, ConnectionCreator.GetConnection(connectionType));
            connectionInfo.ConnectionType = null;
            var info = mapper.Map<ConnectionInfoExternalDto>(connectionInfo);
            logger.LogTrace("Mapped connection info: {@Info}", info);
            var storage = await connection.GetStorage(info);
            return storage;
        }
    }
}
