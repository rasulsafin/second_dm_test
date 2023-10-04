using System.Collections.Generic;
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
    /// Controller for managing BimElements.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class BimElementsController : Controller
    {
        private readonly IBimElementService service;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public BimElementsController(IBimElementService service,
            IStringLocalizer<SharedLocalization> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Returns list of bim elements with computed statuses.
        /// </summary>
        /// <param name="projectID">Project's ID.</param>
        /// <response code="200">Collection of BimElementStatuses.</response>
        /// <response code="400">Invalid project id.</response>
        /// <response code="404">Could not find project to retrieve bim elements list.</response>
        /// <response code="500">Something went wrong while retrieving the objective list.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<BimElementStatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBimElementsStatuses(
            [FromQuery]
            [CheckValidID]
            int projectID)
        {
            try
            {
                var objectives = await service.GetBimElementsStatuses(new ID<ProjectDto>(projectID));
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
    }
}
