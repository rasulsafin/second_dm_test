using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Brio.Docs.Api.Validators;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using static Brio.Docs.Api.Validators.ServiceResponsesValidator;

namespace Brio.Docs.Api.Controllers
{
    /// <summary>
    /// Controller for synchronize with remote connections (e.g. YandexDisk, TDMS, BIM360).
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class SynchronizationsController : ControllerBase
    {
        private readonly ISynchronizationForApiService service;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public SynchronizationsController(
            ISynchronizationForApiService service,
            IStringLocalizer<SharedLocalization> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Synchronize user's data.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Id of the created long request.</returns>
        /// <response code="202">Request is accepted but can take a long time to proceed. Check with the /RequestQueue to get the result.</response>
        /// <response code="400">Invalid user id.</response>
        /// <response code="404">User was not found.</response>
        /// <response code="500">Something went wrong while server tried to synchronize.</response>
        [HttpGet]
        [Route("start/{userID:int}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(RequestID), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Synchronize(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID)
        {
            try
            {
                var result = await service.Synchronize(new ID<UserDto>(userID));
                return Accepted(result);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["CouldNotSynchronize"], ex.Message);
            }
        }

        /// <summary>
        /// Get dates of synchronizations for the user.
        /// </summary>
        /// <returns>The collection of synchronization dates.</returns>
        /// <response code="200">The collection of synchronization dates returned.</response>
        /// <response code="400">Invalid user id.</response>
        /// <response code="404">User was not found.</response>
        /// <response code="500">Something went wrong while server tried to get the date.</response>
        [HttpGet]
        [Route("dates/{userID:int}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSynchronizationsDates(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID)
        {
            try
            {
                var result = await service.GetSynchronizationDates(new ID<UserDto>(userID));
                return Ok(result);
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
        /// Remove the last synchronization date of the user for an attempt to sync entities that were updated earlier than the last sync date.
        /// The entities will not be returned to the previous state.
        /// </summary>
        /// <returns>True, the last synchronization date is removed.</returns>
        /// <response code="200">The last synchronization date removed.</response>
        /// <response code="400">Invalid user id.</response>
        /// <response code="404">User was not found.</response>
        /// <response code="500">Something went wrong while server tried to remove the date.</response>
        [HttpDelete]
        [Route("dates/{userID:int}/last")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveLastSynchronizationDate(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID)
        {
            try
            {
                var result = await service.RemoveLastSynchronizationDate(new ID<UserDto>(userID));
                return Ok(result);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return CreateProblemResult(this, 404, localizer["SomethingIsMissing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Delete"], ex.Message);
            }
        }

        /// <summary>
        /// Removes all synchronization dates of the user for an attempt to synchronize all data.
        /// The entities will not be returned to the previous state.
        /// </summary>
        /// <returns>True, if all synchronization dates are removed.</returns>
        /// <response code="200">All synchronization dates removed.</response>
        /// <response code="400">Invalid user id.</response>
        /// <response code="404">User was not found.</response>
        /// <response code="500">Something went wrong while server tried to remove the date.</response>
        [HttpDelete]
        [Route("dates/{userID:int}/all")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveAllSynchronizationDates(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID)
        {
            try
            {
                var result = await service.RemoveAllSynchronizationDates(new ID<UserDto>(userID));
                return Ok(result);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Delete"], ex.Message);
            }
        }
    }
}
