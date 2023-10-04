using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Brio.Docs.Api.Validators;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services.ForApi;
using Brio.Docs.Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using static Brio.Docs.Api.Validators.ServiceResponsesValidator;

namespace Brio.Docs.Api.Controllers
{
    /// <summary>
    /// Controller for managing Project entities.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectForApiService service;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public ProjectsController(IProjectForApiService service,
            IStringLocalizer<SharedLocalization> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Get projects linked to the specific user.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <returns>Collection of projects.</returns>
        /// <response code="200">List of user's project.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">If user was not found.</response>
        /// <response code="500">Something went wrong while retrieving list of projects.</response>
        [HttpGet]
        [Route("user/{userID}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserProjects(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID)
        {
            try
            {
                var userProjects = await service.GetUserProjects(new ID<UserDto>(userID));
                return Ok(userProjects);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Create new project.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Projects
        ///     {
        ///         "authorID": { "id" : 0},
        ///         "title": "TitleValue",
        ///         "items":
        ///         [
        ///             {
        ///                 "id": { "id" : 0 },
        ///                 "relativePath": "\file.txt",
        ///                 "itemType": 0
        ///             },
        ///             {
        ///                 "id": { "id" : 0 },
        ///                 "relativePath": "\Media\image.png",
        ///                 "itemType": 2
        ///             },
        ///        ]
        ///     }
        /// </remarks>
        /// <param name="project">Project data.</param>
        /// <returns>Created project.</returns>
        /// <response code="201">Project was created.</response>
        /// <response code="400">If project title is null.</response>
        /// <response code="500">Something went wrong while creating the project.</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ProjectToListDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add(
            [FromBody]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Add")]
            ProjectToCreateDto project)
        {
            try
            {
                var projectToReturn = await service.Add(project);
                return Created(string.Empty, projectToReturn);
            }
            catch (ArgumentValidationException ex)
            {
                return CreateProblemResult(
                    this,
                    400,
                    localizer["CheckValidProjectTitleToAdd_TitleRequired"],
                    ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Add"], ex.Message);
            }
        }

        /// <summary>
        /// Delete project by its id.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <returns>True if project was deleted.</returns>
        /// <response code="200">Project was deleted successfully.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Project was not found.</response>
        /// <response code="500">Something went wrong while deleting project.</response>
        [HttpDelete]
        [Route("{projectID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Remove(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int projectID)
        {
            try
            {
                await service.Remove(new ID<ProjectDto>(projectID));
                return Ok(true);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidProjectID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Delete"], ex.Message);
            }
        }

        /// <summary>
        /// Update project's values.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /Projects
        ///      {
        ///         "id": { "id" : 138},
        ///         "title": "NewTitleValue",
        ///         "items": [
        ///         {
        ///             "id": {"id": 0},
        ///             "relativePath": "\file.txt",
        ///             "itemType": 0
        ///         }
        ///         ]
        ///      }
        /// </remarks>
        /// <param name="projectData">Project data to update.</param>
        /// <returns>True, if updated successfully.</returns>
        /// <response code="200">Project was updated successfully.</response>
        /// <response code="404">Could not find project to update.</response>
        /// <response code="400">Some of required project's data is null.</response>
        /// <response code="500">Something went wrong while updating project.</response>
        [HttpPut]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(
            [FromBody]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Put")]
            ProjectDto projectData)
        {
            try
            {
                await service.Update(projectData);
                return Ok(true);
            }
            catch (ArgumentValidationException ex)
            {
                return CreateProblemResult(
                    this,
                    400,
                    localizer["CheckValidProjectTitleToAdd_TitleRequired"],
                    ex.Message);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidProjectID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Put"], ex.Message);
            }
        }

        /// <summary>
        /// Get project by id.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <returns>Found project.</returns>
        /// <response code="200">Project found.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Could not find project.</response>
        /// <response code="500">Something went wrong while retrieving the project.</response>
        [HttpGet]
        [Route("{projectID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int projectID)
        {
            try
            {
                var foundProject = await service.Find(new ID<ProjectDto>(projectID));
                return Ok(foundProject);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidProjectID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Get list of users that have access to this project.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <returns>Collection of users.</returns>
        /// <response code="200">List of users with access to that project.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">If project was not found.</response>
        /// <response code="500">Something went wrong while retrieving list of projects.</response>
        [HttpGet]
        [Route("{projectID}/users")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsers(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int projectID)
        {
            try
            {
                var users = await service.GetUsers(new ID<ProjectDto>(projectID));
                return Ok(users);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidProjectID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Link existing project to users.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Projects/links/138
        ///         [
        ///             {"id": 109},
        ///             {"id": 106},
        ///         ]
        /// </remarks>
        /// <param name="projectID">Project's id.</param>
        /// <param name="users">List of user's ids connect project to.</param>
        /// <returns>True if linked successfully.</returns>
        /// <response code="201">Link created. Return true.</response>
        /// <response code="400">Invalid ids.</response>
        /// <response code="404">If project was not found.</response>
        /// <response code="500">Something went wrong while linking project to users.</response>
        [HttpPost]
        [Route("link/{projectID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LinkToUser(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int projectID,
            [FromBody]
            IEnumerable<ID<UserDto>> users)
        {
            try
            {
                await service.LinkToUsers(new ID<ProjectDto>(projectID), users);
                return Created(string.Empty, true);
            }
            catch (NotFoundException<User> ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (NotFoundException<Project> ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidProjectID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Put"], ex.Message);
            }
        }

        /// <summary>
        /// Unlink existing project from list of users.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Projects/links/138
        ///         [
        ///            {"id": 109},
        ///            {"id": 106},
        ///         ]
        /// </remarks>
        /// <param name="projectID">Project's id.</param>
        /// <param name="users">List of user's ids unlink project from.</param>
        /// <returns>True if unlinked successfully.</returns>
        /// <response code="200">Unlinked.</response>
        /// <response code="400">Invalid ids.</response>
        /// <response code="404">If project was not found.</response>
        /// <response code="500">Something went wrong while unlinking project from users.</response>
        [HttpPost]
        [Route("unlink/{projectID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UnlinkFromUser(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int projectID,
            [FromBody]
            IEnumerable<ID<UserDto>> users)
        {
            try
            {
                await service.UnlinkFromUsers(new ID<ProjectDto>(projectID), users);
                return Ok(true);
            }
            catch (NotFoundException<User> ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (NotFoundException<Project> ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidProjectID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Put"], ex.Message);
            }
        }

        /// <summary>
        /// Get list of all projects.
        /// </summary>
        /// <returns>List of all projects.</returns>
        /// <response code="200">Returns found list of projects.</response>
        /// <response code="500">Something went wrong while retrieving projects.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllProjects()
        {
            try
            {
                var projects = await service.GetAllProjects();
                return Ok(projects);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }
    }
}
