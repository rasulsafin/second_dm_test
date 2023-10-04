using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Dtos.ForApi.Project;
using Brio.Docs.Client.Dtos.ForApi.Projects;
using Brio.Docs.Client.Dtos.ForApi.Users;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services.ForApi;
using Brio.Docs.Client.Services.ForApi.Helpers;
using Brio.Docs.Integration.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Brio.Docs.Services.ForApi
{
    public class ProjectForApiService : IProjectForApiService, IDisposable
    {
        private readonly IMapper mapper;
        private readonly ILogger<ProjectService> logger;
        private readonly IHttpRequestForApiHandlerService httpService;

        public ProjectForApiService(IMapper mapper, ILogger<ProjectService> logger, IHttpRequestForApiHandlerService httpService)
        {
            this.mapper = mapper;

            this.httpService = httpService;

            this.logger = logger;

            logger.LogTrace("ProjectApiService created");
        }

        public async Task<IEnumerable<ProjectToListDto>> GetAllProjects()
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetAllProjects started");
            try
            {
                var response = await httpService.SendGetRequest("api/project");

                var content = await response.Content.ReadAsStringAsync();

                var projects = JsonConvert.DeserializeObject<List<ProjectToListDto>>(content);
                return projects;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get list of all projects");
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<ProjectToListDto>> GetUserProjects(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetAllProjects started");
            try
            {
                var response = await httpService.SendGetRequest($"api/project/for_user/{userID}");
                var content = await response.Content.ReadAsStringAsync();
                var projectsFromApi = JsonConvert.DeserializeObject<List<ProjectToReadForApiDto>>(content);

                var projectsToReturn = projectsFromApi.Select(proj => mapper.Map<ProjectToListDto>(proj)).ToList();

                return projectsToReturn;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get list of projects");
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<ProjectToListDto> Add(ProjectToCreateDto projectToCreate)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with projectToCreate: {@ProjectToCreate}", projectToCreate);
            try
            {
                // 1. Get User From Api.
                var currentUserID = (int)projectToCreate.AuthorID;
                var userFromDpApi = await GetUserFromDmApi(currentUserID);

                // 2. Create project in Api.
                var projToCreateForApi = ConvertToProjectToCreateForApi(projectToCreate, userFromDpApi);

                var createdProj = await SendProjectToCreateToApi(projToCreateForApi);
                return mapper.Map<ProjectToListDto>(createdProj);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't add project {@ProjectToCreate}", projectToCreate);
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        // TODO: Api doesn't have this endpoint
        public async Task<bool> Remove(ID<ProjectDto> projectID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Remove started with projectID: {@ProjectID}", projectID);
            try
            {
                var response = await httpService.SendDeleteRequest($"api/project/{projectID}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't delete project {ProjectID}", projectID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<bool> Update(ProjectDto project)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Update started with project: {@Project}", project);

            try
            {
                var projectToApi = mapper.Map<ProjectToUpdateForApi>(project);

                var response = await httpService.SendPutRequest("api/project", projectToApi);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't update project {@Project}", project);
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<ProjectDto> Find(ID<ProjectDto> projectID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started with projectID: {@ProjectID}", projectID);
            try
            {
                var response = await httpService.SendGetRequest($"api/project/{projectID}");
                var content = await response.Content.ReadAsStringAsync();

                var projFromApi = JsonConvert.DeserializeObject<ProjectToReadForApiDto>(content);

                var projToReturn = mapper.Map<ProjectDto>(projFromApi);

                return projToReturn;
            }
            catch (Exception ex)
            {
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<UserDto>> GetUsers(ID<ProjectDto> projectID)
        {
            return null;
        }

        public async Task<bool> LinkToUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users)
        {
            return true;
        }

        public async Task<bool> UnlinkFromUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users)
        {
            return true;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private ProjectToCreateForApiDto ConvertToProjectToCreateForApi(ProjectToCreateDto projectToCreateDto, UserToReadForApi userFromDbApi)
        {
            var projToCreateForApi = mapper.Map<ProjectToCreateForApiDto>(projectToCreateDto);

            projToCreateForApi.Users = new List<UserForApiDto>() { userFromDbApi };
            projToCreateForApi.UserIds = new List<int>
                {
                    (int)userFromDbApi.Id,
                };

            return projToCreateForApi;
        }

        private async Task<UserToReadForApi> GetUserFromDmApi(int userID)
        {
            var response = await httpService.SendGetRequest($"api/users/{userID}");

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<UserToReadForApi>(content);

        }

        private async Task<ProjectToCreateForApiDto> SendProjectToCreateToApi(ProjectToCreateForApiDto projToCreate)
        {
            try
            {
                var response = await httpService.SendPostRequest("api/project", projToCreate);

                var cont = await response.Content.ReadAsStringAsync();
                var id = JsonConvert.DeserializeObject<int>(cont);
                projToCreate.Id = id;
                return projToCreate;

                throw new Exception($"HTTP Request failed with status code: {response.StatusCode}");

            }
            catch (Exception ex)
            {
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }
    }
}
