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
    /// Controller for managing Connection Types.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ConnectionTypesController : ControllerBase
    {
        private readonly IStringLocalizer<SharedLocalization> localizer;
        private IConnectionTypeService service;

        public ConnectionTypesController(IConnectionTypeService service,
            IStringLocalizer<SharedLocalization> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Add new connection type with given name.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /ConnectionTypes
        ///         "typeNameValue"
        ///
        /// </remarks>
        /// <param name="typeName">Name of the new connection type.</param>
        /// <returns>ID of added connection type.</returns>
        /// <response code="201">Connection type was created.</response>
        /// <response code="400">If type name is null OR type with the same name already exists.</response>
        /// <response code="500">Something went wrong while creating objective type.</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add(
            [FromBody]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Add")]
            string typeName)
        {
            try
            {
                var typeId = await service.Add(typeName);
                return Created(string.Empty, typeId);
            }
            catch (ArgumentValidationException ex)
            {
                return CreateProblemResult(this, 400, localizer["CheckValidConnectionTypeToCreate_AlreadyExists"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Add"], ex.Message);
            }
        }

        /// <summary>
        /// Find a connection type by ID.
        /// </summary>
        /// <param name="id">Type's ID.</param>
        /// <returns>Found connection type.</returns>
        /// <response code="200">Connection Type found.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Could not find Connection Type.</response>
        /// <response code="500">Something went wrong while retrieving the Connection type.</response>
        [HttpGet]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ConnectionTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int id)
        {
            try
            {
                var foundType = await service.Find(new ID<ConnectionTypeDto>(id));
                return Ok(foundType);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidConnectionTypeID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Find a connection type by name.
        /// </summary>
        /// <param name="typename">Type's name.</param>
        /// <returns>Found connection type.</returns>
        /// <response code="200">Connection Type found.</response>
        /// <response code="400">Type name is null.</response>
        /// <response code="404">Could not find Connection Type.</response>
        /// <response code="500">Something went wrong while retrieving the Connection type.</response>
        [HttpGet]
        [Route("name")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ConnectionTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find(
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Add")]
            string typename)
        {
            try
            {
                var foundType = await service.Find(typename);
                return Ok(foundType);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidConnectionTypeID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Get all registered connection types.
        /// </summary>
        /// <returns>Collection of connection types.</returns>
        /// <response code="200">Collection of Connection Types found.</response>
        /// <response code="500">Something went wrong while retrieving the Connection type.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllConnectionTypes()
        {
            try
            {
                var allTypes = await service.GetAllConnectionTypes();
                return Ok(allTypes);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Delete Connection type by id.
        /// </summary>
        /// <param name="id">Connection Type's id.</param>
        /// <returns>True, if deletion was successful.</returns>
        /// <response code="200">Connection Type was deleted successfully.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Connection Type was not found.</response>
        /// <response code="500">Something went wrong while deleting Connection Type.</response>
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Remove(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int id)
        {
            try
            {
                await service.Remove(new ID<ConnectionTypeDto>(id));
                return Ok(true);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidConnectionTypeID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Delete"], ex.Message);
            }
        }

        /// <summary>
        /// Register all existing Connection Types. Use by admin once at the start OR when new connection types are added.
        /// </summary>
        /// <returns>Result of registration.</returns>
        /// <response code="200">Connection Types were registered successfully.</response>
        /// <response code="500">Something went wrong while registering Connection Types.</response>
        [HttpGet]
        [Route("register")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register()
        {
            try
            {
               await service.RegisterAll();
               return Ok(true);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Delete"], ex.Message);
            }
        }
    }
}
