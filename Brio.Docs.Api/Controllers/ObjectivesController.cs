using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Api.Validators;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Filters;
using Brio.Docs.Client.Services;
using Brio.Docs.Client.Services.ForApi;
using Brio.Docs.Client.Sorts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using static Brio.Docs.Api.Validators.ServiceResponsesValidator;

namespace Brio.Docs.Api.Controllers
{
    /// <summary>
    /// Controller for managing Objectives.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ObjectivesController : ControllerBase
    {
        private readonly IObjectiveForApiService service;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public ObjectivesController(IObjectiveForApiService service,
            IStringLocalizer<SharedLocalization> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Add new objective.
        /// </summary>
        /// <param name="data">Data for new objective.</param>
        /// <returns>Added objective.</returns>
        /// <response code="201">Returns created objective.</response>
        /// <response code="400">One/multiple of required values is/are empty.</response>
        /// <response code="500">Something went wrong while creating new objective.</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ObjectiveToListDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add(
            [FromBody]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Add")]
            ObjectiveToCreateDto data)
        {
            try
            {
                var objectiveToList = await service.Add(data);
                return Created(string.Empty, objectiveToList);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Add"], ex.Message);
            }
        }

        /// <summary>
        /// Delete objectives from database by its id.
        /// </summary>
        /// <param name="objectiveID">Objective's ID.</param>
        /// <returns>List of deleted objective's ids.</returns>
        /// <response code="200">Objective was deleted successfully.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Objective was not found.</response>
        /// <response code="500">Something went wrong while deleting Objective.</response>
        [HttpDelete]
        [Route("{objectiveID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Remove(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int objectiveID)
        {
            try
            {
                var removedData = await service.Remove(new ID<ObjectiveDto>(objectiveID)) ?? Enumerable.Empty<ID<ObjectiveDto>>();
                return Ok(removedData);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidObjectiveID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Delete"], ex.Message);
            }
        }

        /// <summary>
        /// Update existing objective.
        /// </summary>
        /// <param name="objectiveData">Objective to  update.</param>
        /// <returns>True if updated successfully.</returns>
        /// <response code="200">Objective was updated successfully.</response>
        /// <response code="400">Some of objective's data is null.</response>
        /// <response code="404">Could not find objective to update.</response>
        /// <response code="500">Something went wrong while updating objective.</response>
        [HttpPut]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(
            [FromBody]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Put")]
            ObjectiveDto objectiveData)
        {
            try
            {
                await service.Update(objectiveData);
                return Ok(true);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidObjectiveID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Put"], ex.Message);
            }
        }

        /// <summary>
        /// Find and return objective by id if exists.
        /// </summary>
        /// <param name="objectiveID">Objective's ID.</param>
        /// <returns>Found objective.</returns>
        /// <response code="200">Objective found.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Could not find objective.</response>
        /// <response code="500">Something went wrong while retrieving the objective.</response>
        [HttpGet]
        [Route("{objectiveID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int objectiveID)
        {
            try
            {
                var foundObjective = await service.Find(new ID<ObjectiveDto>(objectiveID));
                return Ok(foundObjective);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidObjectiveID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Return list of objectives, linked to specific project.
        /// </summary>
        /// <param name="projectID">Project's ID.</param>
        /// <param name="filter">Parameters for filtration.</param>
        /// <param name="sort">Parameter for sorting</param>
        /// <returns>Collection of objectives.</returns>
        /// <response code="200">Collection of objectives linked to project with the pagination info.</response>
        /// <response code="400">Invalid project id.</response>
        /// <response code="404">Could not find project to retrieve objective list.</response>
        /// <response code="500">Something went wrong while retrieving the objective list.</response>
        [HttpPost]
        [Route("project/{projectID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedListDto<ObjectiveToListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetObjectives(
            [FromRoute]
            [CheckValidID]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int projectID,
            [FromBody]
            ObjectiveFilterParameters filter,
            [FromQuery]
            string sort)
        {
            try
            {
                var sortParameters = SortParametersUtils.FromQueryString(sort);
                var objectives = await service.GetObjectives(new ID<ProjectDto>(projectID), filter, sortParameters);
                return Ok(objectives);
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
        /// Return list of sub-objectives, linked to specific parent objective.
        /// </summary>
        /// <param name="parentID">Parent's ID.</param>
        /// <returns>Collection of sub-objectives.</returns>
        /// <response code="200">Collection of sub-objectives linked to objective.</response>
        /// <response code="400">Invalid parent id.</response>
        /// <response code="404">Could not find objective to retrieve objective list.</response>
        /// <response code="500">Something went wrong while retrieving the objective list.</response>
        [HttpGet]
        [Route("subobjectives/{parentID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedListDto<ObjectiveToListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetObjectivesByParent(
            [FromRoute]
            [CheckValidID]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int parentID)
        {
            try
            {
                var objectives = await service.GetObjectivesByParent(new ID<ObjectiveDto>(parentID));
                return Ok(objectives);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidObjectiveID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Return list of objectives, included only ID and BimElements, linked to specific project.
        /// </summary>
        /// <param name="projectID">Project's ID.</param>
        /// <param name="filter">Parameters for filtration.</param>
        /// <returns>Collection of objectives, included only ID and BimElements.</returns>
        /// <response code="200">Collection of objectives, included only ID and BimElements, linked to project.</response>
        /// <response code="400">Invalid project id.</response>
        /// <response code="404">Could not find project to retrieve objective list.</response>
        /// <response code="500">Something went wrong while retrieving the objective list.</response>
        [HttpPost]
        [Route("ids/{projectID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<ID<ObjectiveToSelectionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetObjectivesForSelection(
            [FromRoute]
            [CheckValidID]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int projectID,
            [FromBody]
            ObjectiveFilterParameters filter)
        {
            try
            {
                var objectives = await service.GetObjectivesForSelection(new ID<ProjectDto>(projectID), filter);
                return Ok(objectives);
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
        /// Return list of objectives with locations, linked to specific project and bound to given item.
        /// </summary>
        /// <param name="projectID">Project's ID.</param>
        /// <param name="itemName">Parameters for location filtration.</param>
        /// <param name="filter">Parameters for filtration.</param>
        /// <returns>Collection of objectives.</returns>
        /// <response code="200">Collection of objectives linked to project with locations bound to given item.</response>
        /// <response code="400">Invalid project id.</response>
        /// <response code="404">Could not find project to retrieve objective list.</response>
        /// <response code="500">Something went wrong while retrieving the objective list.</response>
        [HttpPost]
        [Route("locations")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<ObjectiveToLocationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetObjectivesWithLocations(
            [FromQuery]
            [CheckValidID]
            int projectID,
            [FromQuery]
            string itemName,
            [FromBody]
            ObjectiveFilterParameters filter)
        {
            try
            {
                var objectives = await service.GetObjectivesWithLocation(new ID<ProjectDto>(projectID), itemName, filter);
                return Ok(objectives);
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
        /// Return all bim parents of objectives linked to project.
        /// </summary>
        /// <param name="projectID">Project's ID.</param>
        /// <returns>Collection of bim parents.</returns>
        /// <response code="200">Collection of sub-objectives linked to objective.</response>
        /// <response code="400">Invalid project id.</response>
        /// <response code="404">Could not find bim parents.</response>
        /// <response code="500">Something went wrong while retrieving bim parents list.</response>
        [HttpGet]
        [Route("bimparents/{projectID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetObjectiveBimParents(
            [FromRoute]
            [CheckValidID]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int projectID)
        {
            try
            {
                var parents = await service.GetParentsOfObjectivesBimElements(new ID<ProjectDto>(projectID));
                return Ok(parents);
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
    }
}
