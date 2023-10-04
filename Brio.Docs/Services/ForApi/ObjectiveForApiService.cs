using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Dtos.ForApi.Records;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Filters;
using Brio.Docs.Client.Services.ForApi;
using Brio.Docs.Client.Services.ForApi.Helpers;
using Brio.Docs.Client.Sorts;
using Brio.Docs.Common.ForApi;
using Brio.Docs.Database.Models;
using Brio.Docs.Utility.Pagination;
using Brio.Docs.Utility.Sorting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Brio.Docs.Services.ForApi
{
    public class ObjectiveForApiService : IObjectiveForApiService, IDisposable
    {
        private readonly IMapper mapper;
        private readonly IHttpRequestForApiHandlerService httpService;
        private readonly QueryMapper<Objective> queryMapper;

        public ObjectiveForApiService(IMapper mapper, IHttpRequestForApiHandlerService httpService)
        {
            this.mapper = mapper;
            this.httpService = httpService;

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

        public async Task<ObjectiveToListDto> Add(ObjectiveToCreateDto data)
        {
            var objToCreate = mapper.Map<RecordToCreateForApiDto>(data);

            // Assigning some values.

            // Suppose priority is normal. (default value)
            objToCreate.Priority = PriorityEnum.Medium;

            var response = await httpService.SendPostRequest("api/record", objToCreate);

            if (response.IsSuccessStatusCode)
            {
                return mapper.Map<ObjectiveToListDto>(objToCreate);
            }
            else
            {
                throw new Exception("Something went wrong on a remote API server.");
            }
        }

        public async Task<IEnumerable<ID<ObjectiveDto>>> Remove(ID<ObjectiveDto> objectiveID)
        {
            var response = await httpService.SendDeleteRequest($"api/record/{(long)objectiveID}");
            return new List<ID<ObjectiveDto>>()
            {
                objectiveID,
            };
        }

        public async Task<bool> Update(ObjectiveDto objectiveData)
        {
            try
            {
                var objFromApi = await Find(objectiveData.ID);
                var recForApi = mapper.Map<RecordForApiDto>(objectiveData);

                var response = await httpService.SendPutRequest("api/record", recForApi);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                // TODO: Make work with errors
                throw new DocumentManagementException("Some text");
            }
        }

        public async Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID)
        {
            var response = await httpService.SendGetRequest($"api/record/{objectiveID}");
            var content = await response.Content.ReadAsStringAsync();
            var dataFromApi = JsonConvert.DeserializeObject<RecordToReadForApiDto>(content);

            var newObj = mapper.Map<ObjectiveDto>(dataFromApi);

            return newObj;

            throw new DocumentManagementException("Something went wrong...");
        }

        public async Task<PagedListDto<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectId, ObjectiveFilterParameters filter, SortParameters sort)
        {
            var response = await httpService.SendGetRequest($"api/record/get_records/{projectId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var records = JsonConvert.DeserializeObject<IEnumerable<RecordToReadForApiDto>>(content);

                var objectivesToReturn = records
                    .Select(rec => mapper.Map<ObjectiveToListDto>(rec))
                    .ToList();

                if (sort.Sorts != null && sort.Sorts.Any())
                {
                    foreach (var sortParameter in sort.Sorts)
                    {
                        if (sortParameter.IsDescending)
                        {
                            objectivesToReturn = objectivesToReturn.OrderByDescending(dto => GetPropertyValue(dto, sortParameter.FieldName)).ToList();
                        }
                        else
                        {
                            objectivesToReturn = objectivesToReturn.OrderBy(dto => GetPropertyValue(dto, sortParameter.FieldName)).ToList();
                        }
                    }
                }

                return new PagedListDto<ObjectiveToListDto> { PageData = new PagedDataDto() { PageSize = filter.PageSize, TotalCount = objectivesToReturn.Count }, Items = objectivesToReturn };
            }
            else
            {
                throw new DocumentManagementException("ObjectiveForApiService.GetObjectives - exception");
            }
        }

        // TODO:
        private async Task<PagedListDto<ObjectiveToListDto>> SortAndFilterObjectives(IQueryable<Objective> allObjectives, ObjectiveFilterParameters filter, SortParameters sort, ID<ProjectDto> projectID)
        {

            allObjectives = await ApplyFilter(filter, allObjectives, (int)projectID);

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


        public async Task<IEnumerable<ObjectiveToSelectionDto>> GetObjectivesForSelection(ID<ProjectDto> projectID, ObjectiveFilterParameters filter)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ObjectiveToLocationDto>> GetObjectivesWithLocation(ID<ProjectDto> projectID, string itemName, ObjectiveFilterParameters filter)
        {
            // TODO: Need Location Entity from API
            return new List<ObjectiveToLocationDto>();
        }

        public async Task<IEnumerable<SubobjectiveDto>> GetObjectivesByParent(ID<ObjectiveDto> parentID)
        {
            var response = await httpService.SendGetRequest($"api/record/subobjectives/{parentID}");

            var content = await response.Content.ReadAsStringAsync();
            var recordsFromApi = JsonConvert.DeserializeObject<IEnumerable<RecordToReadForApiDto>>(content);

            var subObjectivesToReturn = recordsFromApi.Select(rec => mapper.Map<SubobjectiveDto>(rec)).ToList();
            return subObjectivesToReturn;
        }

        public async Task<IEnumerable<ObjectiveBimParentDto>> GetParentsOfObjectivesBimElements(ID<ProjectDto> projectID)
        {
            return new List<ObjectiveBimParentDto>();
        }

        private object GetPropertyValue(ObjectiveToListDto dto, string propertyName)
        {
            var propertyInfo = typeof(ObjectiveToListDto).GetProperty(propertyName);
            return propertyInfo.GetValue(dto, null);
        }

        private async Task GetAllObjectiveIds(Objective obj, List<int> ids)
        {
            // TODO
        }


        private async Task<IQueryable<Objective>> ApplyFilter(ObjectiveFilterParameters filter, IEnumerable<Objective> filterdObjectives, int projectId)
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

            /*if (filter.ExceptChildrenOf.HasValue && filter.ExceptChildrenOf.Value != 0)
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
            }*/

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

            return (IQueryable<Objective>)filterdObjectives;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
