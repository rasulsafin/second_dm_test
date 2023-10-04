using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Dtos.ForApi.Items;
using Brio.Docs.Client.Dtos.ForApi.Project;
using Brio.Docs.Client.Dtos.ForApi.Projects;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services;
using Brio.Docs.Client.Services.ForApi;
using Brio.Docs.Client.Services.ForApi.Helpers;
using Brio.Docs.Database.Models;
using Brio.Docs.Interfaces;
using Newtonsoft.Json;

namespace Brio.Docs.Services.ForApi
{
    public class ItemForApiService : IItemForApiService
    {
        private readonly IMapper mapper;

        private readonly IHttpRequestForApiHandlerService httpService;

        private readonly IRequestForApiService requestQueue;

        public ItemForApiService(IMapper mapper, IHttpRequestForApiHandlerService httpService, IRequestForApiService requestQueue)
        {
            this.requestQueue = requestQueue;
            this.mapper = mapper;
            this.httpService = httpService;
        }

        public async Task<bool> Update(ItemDto item)
        {
            throw new NotImplementedException();
        }

        public async Task<ItemDto> Find(ID<ItemDto> itemID)
        {
            var response = await httpService.SendGetRequest($"api/item/getById/{(int)itemID}");
            var cont = await response.Content.ReadAsStringAsync();

            var idFromApi = JsonConvert.DeserializeObject<ItemForApiDto>(cont);

            var res = mapper.Map<ItemDto>(idFromApi);

            return res;
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID)
        {
            var response = await httpService.SendGetRequest($"api/project/{projectID}");

            var content = await response.Content.ReadAsStringAsync();
            var projectFromApi = JsonConvert.DeserializeObject<ProjectToReadForApiDto>(content);
            var projectDto = mapper.Map<ProjectDto>(projectFromApi);

            return projectDto.Items;
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID)
        {
            throw new NotImplementedException();
        }

        public async Task<ID<ItemDto>> LinkItem(ID<ProjectDto> projectId, ItemDto itemDto)
        {
            try
            {
                var itemForApi = mapper.Map<ItemForApiDto>(itemDto);

                // If ProjectId is null then HttpRequest doesn't work.
                itemForApi.ProjectId = (int)projectId;
                var response = await httpService.SendPutRequest($"api/item/link_item/{(int)projectId}", itemForApi);
                var content = await response.Content.ReadAsStringAsync();
                var itemId = JsonConvert.DeserializeObject<int>(content);

                return new ID<ItemDto>(itemId);
            }
            catch (Exception ex)
            {
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<RequestID> DownloadItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds)
        {
            try
            {
                var idToApi = mapper.Map<int>(userID);
                var itemIdsToApi = mapper.Map<IEnumerable<int>>(itemIds);

                var response = await httpService.SendPostRequest($"api/item/download_files/{idToApi}", itemIdsToApi);
                var id = Guid.NewGuid().ToString();
                var src = new CancellationTokenSource();

                var task = Task.Factory.StartNew(async () =>
                {
                    return new RequestResult(true);
                });

                requestQueue.AddRequest(id, task.Unwrap(), src);

                if (response.IsSuccessStatusCode)
                {
                    return new RequestID(id);
                }
                else
                {
                    throw new Exception("Something went wrong when uploading a file.");
                }
            }
            catch (Exception ex)
            {
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<RequestID> UploadItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds)
        {
            try
            {
                var idToApi = mapper.Map<int>(userID);
                var itemIdsToApi = mapper.Map<IEnumerable<int>>(itemIds);

                var response = await httpService.SendPostRequest($"api/item/upload_files/{idToApi}", itemIdsToApi);
                var id = Guid.NewGuid().ToString();
                var src = new CancellationTokenSource();

                var task = Task.Factory.StartNew(async () =>
                {
                    return new RequestResult(true);
                });

                requestQueue.AddRequest(id, task.Unwrap(), src);

                if (response.IsSuccessStatusCode)
                {
                    return new RequestID(id);
                }
                else
                {
                    throw new Exception("Something went wrong when uploading a file.");
                }
            }
            catch (Exception ex)
            {
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<RequestID> DeleteItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds)
        {
            throw new NotImplementedException();
        }

    }
}
