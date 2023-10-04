using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Filters;
using Brio.Docs.Client.Services;
using Brio.Docs.Client.Sorts;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Extensions;
using Brio.Docs.Utility.Pagination;
using Brio.Docs.Utility.Sorting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Services
{
    public class ObjectiveService : IObjectiveService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ItemsHelper itemHelper;
        private readonly DynamicFieldsHelper dynamicFieldHelper;
        private readonly BimElementsHelper bimElementHelper;
        private readonly ILogger<ObjectiveService> logger;
        private readonly QueryMapper<Objective> queryMapper;

        public ObjectiveService(DMContext context,
            IMapper mapper,
            ItemsHelper itemHelper,
            DynamicFieldsHelper dynamicFieldHelper,
            BimElementsHelper bimElementHelper,
            ILogger<ObjectiveService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.itemHelper = itemHelper;
            this.dynamicFieldHelper = dynamicFieldHelper;
            this.bimElementHelper = bimElementHelper;
            this.logger = logger;
            logger.LogTrace("ObjectiveService created");

            queryMapper = new QueryMapper<Objective>(new QueryMapperConfiguration { IsCaseSensitive = false, IgnoreNotMappedFields = false });
            queryMapper.AddMap(nameof(ObjectiveToListDto.Status), x => x.Status);
            queryMapper.AddMap(nameof(ObjectiveToListDto.Title), x => x.TitleToLower);
            queryMapper.AddMap(nameof(Objective.CreationDate), x => x.CreationDate);
            queryMapper.AddMap(nameof(Objective.UpdatedAt), x => x.UpdatedAt);
            queryMapper.AddMap(nameof(Objective.DueDate), x => x.DueDate);
            queryMapper.AddMap("CreationDateDateOnly", x => x.CreationDate.Date);
            queryMapper.AddMap("UpdatedAtDateOnly", x => x.UpdatedAt.Date);
            queryMapper.AddMap("DueDateDateOnly", x => x.DueDate.Date);
        }

        public async Task<ObjectiveToListDto> Add(ObjectiveToCreateDto objectiveToCreate)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with data: {@Data}", objectiveToCreate);
            try
            {
                if (!objectiveToCreate.AuthorID.HasValue)
                    throw new ArgumentException("The author id is required");

                var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);
                logger.LogTrace("Mapped data: {@Objective}", objectiveToSave);
                await context.Objectives.AddAsync(objectiveToSave);
                await context.SaveChangesAsync();

                await bimElementHelper.AddBimElementsAsync(objectiveToCreate.BimElements, objectiveToSave);
                await itemHelper.AddItemsAsync(objectiveToCreate.Items, objectiveToSave);
                await dynamicFieldHelper.AddDynamicFieldsAsync(
                    objectiveToCreate.DynamicFields,
                    objectiveToSave,
                    objectiveToCreate.AuthorID.Value);
                await AddLocationAsync(objectiveToCreate.Location, objectiveToSave);

                return mapper.Map<ObjectiveToListDto>(objectiveToSave);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't add objective {@Data}", objectiveToCreate);
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<Objective> AddLocationAsync(LocationDto locationDto, Objective objective)
        {
            var location = mapper.Map<Location>(locationDto);
            if (location != null)
            {
                var locationItemDto = locationDto?.Item;
                if (locationItemDto != null)
                {
                    objective.Project ??= await context.Projects.Include(x => x.Items)
                       .FindOrThrowAsync(x => x.ID, objective.ProjectID);

                    var locationItem = await itemHelper.CheckItemToLink(
                        locationItemDto,
                        new ProjectItemContainer(objective.Project));

                    if (locationItem != null)
                        objective.Project.Items.Add(locationItem);
                    else
                        locationItem = await context.FindOrThrowAsync<Item>((int)locationItemDto.ID);

                    objective.Location = location;
                    objective.Location.Item = locationItem;
                }

                await context.SaveChangesAsync();
            }

            return objective;
        }

        public async Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with objectiveID: {@ObjectiveID}", objectiveID);
            try
            {
                var dbObjective = await GetOrThrowAsync(objectiveID);
                logger.LogDebug("Found: {@DBObjective}", dbObjective);
                var objective = mapper.Map<ObjectiveDto>(dbObjective);
                logger.LogDebug("Created DTO: {@Objective}", objective);
                return objective;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get objective with key {ObjectiveID}", objectiveID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<PagedListDto<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectID, ObjectiveFilterParameters filter, SortParameters sort)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetObjectives started with projectID: {@ProjectID}", projectID);
            try
            {
                var dbProject = await context.Projects.Unsynchronized()
                    .FindOrThrowAsync(x => x.ID, (int)projectID);
                logger.LogDebug("Found project: {@DBProject}", dbProject);

                var allObjectives = context.Objectives
                                    .AsNoTracking()
                                    .Unsynchronized()
                                    .Where(x => x.ProjectID == dbProject.ID);

                allObjectives = await ApplyFilter(filter, allObjectives, dbProject.ID);

                var totalCount = allObjectives != null ? await allObjectives.CountAsync() : 0;
                var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

                var objectives = await allObjectives?
                    .SortWithParameters(sort, queryMapper, x => x.CreationDate)
                    .ByPages(filter.PageNumber, filter.PageSize)
                    .Include(x => x.ObjectiveType)
                    .Include(x => x.BimElements)
                        .ThenInclude(x => x.BimElement)
                    .Include(x => x.Location)
                        .ThenInclude(x => x.Item)
                    .Select(x => mapper.Map<ObjectiveToListDto>(x))
                    .ToListAsync();

                return new PagedListDto<ObjectiveToListDto>()
                {
                    Items = objectives ?? Enumerable.Empty<ObjectiveToListDto>(),
                    PageData = new PagedDataDto()
                    {
                        CurrentPage = filter.PageNumber,
                        PageSize = filter.PageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                    },
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't find objectives by project key {ProjectID}", projectID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<ObjectiveToSelectionDto>> GetObjectivesForSelection(ID<ProjectDto> projectID, ObjectiveFilterParameters filter)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetObjectives with IDs and BIM-elements started with projectID: {@ProjectID}", projectID);
            try
            {
                var dbProject = await context.Projects.Unsynchronized()
                    .FindOrThrowAsync(x => x.ID, (int)projectID);
                logger.LogDebug("Found project: {@DBProject}", dbProject);

                var allObjectives = context.Objectives
                                    .AsNoTracking()
                                    .Unsynchronized()
                                    .Where(x => x.ProjectID == dbProject.ID);

                allObjectives = await ApplyFilter(filter, allObjectives, dbProject.ID);

                var objectives = await allObjectives?
                    .Include(x => x.BimElements)
                            .ThenInclude(x => x.BimElement)
                    .Select(x => mapper.Map<ObjectiveToSelectionDto>(x))
                    .ToListAsync();

                return objectives;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't find objectives by project key {ProjectID}", projectID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<ObjectiveToLocationDto>> GetObjectivesWithLocation(ID<ProjectDto> projectID, string itemName, ObjectiveFilterParameters filter)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetObjectivesWithLocation started with projectID: {@ProjectID}", projectID);
            try
            {
                var dbProject = await context.Projects.Unsynchronized()
                    .FindOrThrowAsync(x => x.ID, (int)projectID);
                logger.LogDebug("Found project: {@DBProject}", dbProject);

                var objectivesWithLocations = context.Objectives
                                    .AsNoTracking()
                                    .Unsynchronized()
                                    .Where(x => x.ProjectID == dbProject.ID)
                                    .Include(x => x.Location)
                                        .ThenInclude(x => x.Item)
                                    .Where(x => x.Location != null);

                if (!string.IsNullOrEmpty(itemName))
                    objectivesWithLocations = objectivesWithLocations.Where(x => x.Location.Item.Name == itemName);

                objectivesWithLocations = await ApplyFilter(filter, objectivesWithLocations, dbProject.ID);

                return await objectivesWithLocations
                    .Select(x => mapper.Map<ObjectiveToLocationDto>(x))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't find objectives by project key {ProjectID}", projectID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<ID<ObjectiveDto>>> Remove(ID<ObjectiveDto> objectiveID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Remove started with objectiveID: {@ObjectiveID}", objectiveID);
            try
            {
                var objective = await context.Objectives.FindOrThrowAsync((int)objectiveID);
                logger.LogDebug("Found objective: {@Objective}", objective);

                var deletedIds = new List<int>();
                await GetAllObjectiveIds(objective, deletedIds);

                context.Objectives.Remove(objective);
                await context.SaveChangesAsync();

                return deletedIds.Select(id => new ID<ObjectiveDto>(id));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't remove objective with key {ObjectiveID}", objectiveID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<bool> Update(ObjectiveDto objectiveDto)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Update started with objData: {@ObjData}", objectiveDto);
            try
            {
                var objectiveFromDb = await context.Objectives.FindOrThrowAsync((int)objectiveDto.ID);
                objectiveFromDb = mapper.Map(objectiveDto, objectiveFromDb);

                await dynamicFieldHelper.UpdateDynamicFieldsAsync(
                    objectiveDto.DynamicFields.Where(x => x.ID.IsValid),
                    objectiveFromDb.ID);
                await dynamicFieldHelper.AddDynamicFieldsAsync(
                    objectiveDto.DynamicFields.Where(x => !x.ID.IsValid),
                    objectiveFromDb,
                    new ID<UserDto>(CurrentUser.ID));
                await bimElementHelper.UpdateBimElementsAsync(objectiveDto.BimElements, objectiveFromDb.ID);
                await itemHelper.UpdateItemsAsync(objectiveDto.Items, objectiveFromDb);

                context.Update(objectiveFromDb);
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't update objective {@ObjData}", objectiveDto);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<SubobjectiveDto>> GetObjectivesByParent(ID<ObjectiveDto> parentID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetObjectivesByParent started with parentID: {@parentID}", parentID);
            try
            {
                var objectivesWithParent = await context.Objectives
                                    .AsNoTracking()
                                    .Unsynchronized()
                                    .Where(x => x.ParentObjectiveID == (int)parentID)
                                    .OrderBy(x => x.CreationDate)
                                    .Select(x => mapper.Map<SubobjectiveDto>(x))
                                    .ToListAsync();

                return objectivesWithParent;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't find objectives by parentID key {parentID}", parentID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<ObjectiveBimParentDto>> GetParentsOfObjectivesBimElements(ID<ProjectDto> projectID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Get parents of bim elements in all objectives by projectID: {@ProjectID}", projectID);
            try
            {
                var dbProject = await context.Projects.Unsynchronized()
                    .FindOrThrowAsync(x => x.ID, (int)projectID);
                logger.LogDebug("Found project: {@DBProject}", dbProject);

                var bimParents = await context.BimElementObjectives.AsNoTracking()
                    .Where(x => x.Objective.ProjectID == dbProject.ID)
                    .Where(x => !x.Objective.IsSynchronized)
                    .Select(x => x.BimElement.ParentName)
                    .Distinct()
                    .ToListAsync();

                return bimParents.Select(x => new ObjectiveBimParentDto() { ParentName = x }).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't find objectives by project key {ProjectID}", projectID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        private async Task<Objective> GetOrThrowAsync(ID<ObjectiveDto> objectiveID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Get started for objective {ID}", objectiveID);
            var dbObjective = await context.Objectives
                .Unsynchronized()
                .Include(x => x.Project)
                .Include(x => x.Author)
                .Include(x => x.ObjectiveType)
                .Include(x => x.Location)
                     .ThenInclude(x => x.Item)
                .Include(x => x.DynamicFields)
                     .ThenInclude(x => x.ChildrenDynamicFields)
                .Include(x => x.Items)
                     .ThenInclude(x => x.Item)
                .Include(x => x.BimElements)
                     .ThenInclude(x => x.BimElement)
                .FindOrThrowAsync(x => x.ID, (int)objectiveID);

            logger.LogDebug("Found objective: {@DBObjective}", dbObjective);

            return dbObjective;
        }

        private async Task GetAllObjectiveIds(Objective obj, List<int> ids)
        {
            ids.Add(obj.ID);

            var children = await context.Objectives
                .Unsynchronized()
                .Where(x => x.ParentObjectiveID == obj.ID)
                .ToListAsync();

            foreach (var child in children)
                await GetAllObjectiveIds(child, ids);
        }

        private async Task<IQueryable<Objective>> ApplyFilter(ObjectiveFilterParameters filter, IQueryable<Objective> filterdObjectives, int projectId)
        {
            if (filter.TypeIds != null && filter.TypeIds.Count > 0)
                filterdObjectives = filterdObjectives.Where(x => filter.TypeIds.Contains(x.ObjectiveTypeID));

            if (!string.IsNullOrEmpty(filter.BimElementGuid))
                filterdObjectives = filterdObjectives.Where(x => x.BimElements.Any(e => e.BimElement.GlobalID == filter.BimElementGuid));

            if (!string.IsNullOrWhiteSpace(filter.TitlePart))
                filterdObjectives = filterdObjectives.Where(x => x.TitleToLower.Contains(filter.TitlePart.ToLower()));

            if (filter.Statuses != null && filter.Statuses.Count > 0)
                filterdObjectives = filterdObjectives.Where(x => filter.Statuses.Contains(x.Status));

            if (filter.CreatedBefore.HasValue)
                filterdObjectives = filterdObjectives.Where(x => x.CreationDate < filter.CreatedBefore.Value);

            if (filter.CreatedAfter.HasValue)
                filterdObjectives = filterdObjectives.Where(x => x.CreationDate >= filter.CreatedAfter.Value);

            if (filter.UpdatedBefore.HasValue)
                filterdObjectives = filterdObjectives.Where(x => x.UpdatedAt < filter.UpdatedBefore.Value);

            if (filter.UpdatedAfter.HasValue)
                filterdObjectives = filterdObjectives.Where(x => x.UpdatedAt >= filter.UpdatedAfter.Value);

            if (filter.FinishedBefore.HasValue)
                filterdObjectives = filterdObjectives.Where(x => x.DueDate < filter.FinishedBefore.Value);

            if (filter.FinishedAfter.HasValue)
                filterdObjectives = filterdObjectives.Where(x => x.DueDate >= filter.FinishedAfter.Value);

            if (filter.ExceptChildrenOf.HasValue && filter.ExceptChildrenOf.Value != 0)
            {
                var obj = await context.Objectives
                    .AsNoTracking()
                    .Unsynchronized()
                    .Where(x => x.ProjectID == projectId)
                    .FirstOrDefaultAsync(o => o.ID == (int)filter.ExceptChildrenOf);

                var childrenIds = new List<int>();
                if (obj != null)
                    await GetAllObjectiveIds(obj, childrenIds);

                filterdObjectives = filterdObjectives.Where(x => !childrenIds.Contains(x.ID));
            }

            if (filter.DynamicFieldValues != null && filter.DynamicFieldValues.Count > 0)
            {
                foreach (var filterValue in filter.DynamicFieldValues)
                {
                    filterdObjectives = filterdObjectives.Where(x =>
                        x.DynamicFields.Any(df => df.ExternalID == filterValue.ExternalId && df.Value == filterValue.Value));
                }
            }

            if (!string.IsNullOrEmpty(filter.BimElementParent))
                filterdObjectives = filterdObjectives.Where(x => x.BimElements.Any(e => e.BimElement.ParentName == filter.BimElementParent));

            return filterdObjectives;
        }
    }
}
