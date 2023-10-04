using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Brio.Docs.Api.Validators;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services;
using Brio.Docs.Client.Services.ForApi;
using Brio.Docs.Common.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using static Brio.Docs.Api.Validators.ServiceResponsesValidator;

namespace Brio.Docs.Api.Controllers
{
    /// <summary>
    /// Controller for managing files/items.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IItemForApiService service;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public ItemsController(IItemForApiService service,
            IStringLocalizer<SharedLocalization> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Links item to the project.
        /// </summary>
        /// <param name="projectID">The project ID.</param>
        /// <param name="item">The linking item.</param>
        /// <returns>The ID of linked item.</returns>
        /// <response code="200">The item was linked successfully.</response>
        /// <response code="400">Some is incorrect.</response>
        /// <response code="404">Could not find project to the linking.</response>
        /// <response code="500">Something went wrong while linking an item.</response>
        [HttpPut]
        [Route("project/{projectID:int}/")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ID<ItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LinkItem(
            [FromRoute]
            [CheckValidID]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int projectID,
            [FromBody]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Put")]
            ItemDto item)
        {
            try
            {
                var result = await service.LinkItem(new ID<ProjectDto>(projectID), item);
                return Ok(result);
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
        /// Updates item.
        /// </summary>
        /// <param name="item">Data to update.</param>
        /// <returns>True if updated.</returns>
        /// <response code="200">Item was updated successfully.</response>
        /// <response code="400">Some of item's data is null.</response>
        /// <response code="404">Could not find item to update.</response>
        /// <response code="500">Something went wrong while updating item.</response>
        [HttpPut]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(
            [FromBody]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Put")]
            ItemDto item)
        {
            try
            {
                await service.Update(item);
                return Ok(true);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidItemID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Put"], ex.Message);
            }
        }

        /// <summary>
        /// Finds item in db.
        /// </summary>
        /// <param name="itemID">Id of item to find.</param>
        /// <returns>Found item.</returns>
        /// <response code="200">Item found.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Could not find item.</response>
        /// <response code="500">Something went wrong while retrieving the item.</response>
        [HttpGet]
        [Route("{itemID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find(
            [FromRoute]
            [CheckValidID]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int itemID)
        {
            try
            {
                var foundItem = await service.Find(new ID<ItemDto>(itemID));
                return Ok(foundItem);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidItemID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Gets list of items that belongs to that project.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <returns>Collection of items.</returns>
        /// <response code="200">Collection of items linked to project.</response>
        /// <response code="400">Invalid project id.</response>
        /// <response code="404">Could not find project to get items.</response>
        /// <response code="500">Something went wrong while trying to get list of items.</response>
        [HttpGet]
        [Route("project/{projectID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ConnectionStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProjectItems(
            [FromRoute]
            [CheckValidID]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int projectID)
        {
            try
            {
                var items = await service.GetItems(new ID<ProjectDto>(projectID));
                return Ok(items);
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
        /// Gets list of items that belongs to that objective.
        /// </summary>
        /// <param name="objectiveID">Objective's id.</param>
        /// <returns>Collection of items.</returns>
        /// <response code="200">Collection of items linked to objective.</response>
        /// <response code="400">Invalid objective id.</response>
        /// <response code="404">Could not find objective to get items.</response>
        /// <response code="500">Something went wrong while trying to get list of items.</response>
        [HttpGet]
        [Route("objective/{objectiveID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ConnectionStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetObjectiveItems(
            [FromRoute]
            [CheckValidID]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int objectiveID)
        {
            try
            {
                var items = await service.GetItems(new ID<ObjectiveDto>(objectiveID));
                return Ok(items);
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
        /// Download files from remote connection to local storage.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Items/{userID}
        ///     [
        ///        {"id": 1},
        ///        {"id": 2},
        ///        {"id": 3}
        ///     ]
        /// </remarks>
        /// <param name="userID">User's ID.</param>
        /// <param name="itemIds">List of items' id from database.</param>
        /// <returns>Id of the created long request.</returns>
        /// <response code="202">Request is accepted but can take a long time to proceed. Check with the /RequestQueue to get the result.</response>
        /// <response code="500">Something went wrong while server tried to download files.</response>
        [HttpPost]
        [Route("{userID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(RequestID), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadItems(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID,
            [FromBody]
            IEnumerable<ID<ItemDto>> itemIds)
        {
            try
            {
                var result = await service.DownloadItems(new ID<UserDto>(userID), itemIds);
                return Accepted(result);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["CouldNotDownload"], ex.Message);
            }
        }

        /// <summary>
        /// Delete items from remote connection.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="itemIds">List of items' id from database.</param>
        /// <returns>True if deleted successfully.</returns>
        /// <response code="200">Items were deleted successfully.</response>
        /// <response code="400">Invalid data.</response>
        /// <response code="404">One or more items were not found.</response>
        /// <response code="500">Something went wrong while deleting items.</response>
        /// <response code="501">Method is not implemented yet.</response>
        [HttpPost]
        [Route("delete/{userID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status501NotImplemented)]
        public async Task<IActionResult> DeleteItems(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID,
            [FromBody]
            IEnumerable<ID<ItemDto>> itemIds)
        {
            try
            {
                var result = await service.DeleteItems(new ID<UserDto>(userID), itemIds);
                return Accepted(result);
            }
            catch (NotImplementedException ex)
            {
                return CreateProblemResult(this, 501, localizer["MethodIsNotImplemented"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Delete"], ex.Message);
            }
        }

        /// <summary>
        /// Upload files from the local storage to the remote connection.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Items/upload/{userID}
        ///     [
        ///        {"id": 1},
        ///        {"id": 2},
        ///        {"id": 3}
        ///     ]
        /// </remarks>
        /// <param name="userID">User's ID.</param>
        /// <param name="itemIds">List of items' id from database.</param>
        /// <returns>Id of the created long request.</returns>
        /// <response code="202">Request is accepted but can take a long time to proceed. Check with the /RequestQueue to get the result.</response>
        /// <response code="500">Something went wrong while server tried to upload files.</response>
        [HttpPost]
        [Route("upload/{userID:int}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(RequestID), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadItems(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID,
            [FromBody]
            IEnumerable<ID<ItemDto>> itemIds)
        {
            try
            {
                var result = await service.UploadItems(new ID<UserDto>(userID), itemIds);
                return Accepted(result);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["FailedToUploadFiles"], ex.Message);
            }
        }
    }
}
