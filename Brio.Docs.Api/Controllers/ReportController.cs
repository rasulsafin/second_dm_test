using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Brio.Docs.Api.Validators;
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
    /// Controller for managing reports.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService reportService;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public ReportController(
            IReportService reportService,
            IStringLocalizer<SharedLocalization> localizer)
        {
            this.reportService = reportService;
            this.localizer = localizer;
        }

        /// <summary>
        /// Generate report about selected objectives.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Objectives/report? reportTypeId=8b4d6eb661884a2b9843f5696a5467a9 % path=C:\\Temp % userID=1 % projectName=ProjectName
        ///     [
        ///        {"id": "1"},
        ///        {"id": "2"},
        ///        {"id": "3"}
        ///     ]
        /// </remarks>
        /// <param name="reportTypeId">Report type identifier.</param>
        /// <param name="report">Report.</param>
        /// <param name="projectDirectory">Path to project directory.</param>
        /// <param name="userID">ID of the user, who generates the report.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <returns>Object representing the result of report creation process.</returns>
        /// <response code="201">Returns objective report creation result.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">One of the objectives is missing.</response>
        /// <response code="500">Something went wrong while generating report.</response>
        [HttpPost]
        [Route("create")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ObjectiveReportCreationResultDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateReport(
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            string reportTypeId,
            [FromBody]
            ReportDto report,
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_PathIsRequired")]
            string projectDirectory,
            [FromQuery]
            [CheckValidID]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int userID,
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_ProjectNameIsRequired")]
            string projectName)
        {
            try
            {
                var result = await reportService.GenerateReport(reportTypeId, report, projectDirectory, userID, projectName);
                return Created(string.Empty, result);
            }
            catch (ArgumentValidationException ex)
            {
                return CreateProblemResult(this, 400, localizer["FailedToCreateReport"], ex.Message);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["FailedToCreateReport"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["FailedToCreateReport"], ex.Message);
            }
        }

        /// <summary>
        /// Retrieve information about available report types.
        /// </summary>
        /// <returns>Collection of available report types.</returns>
        [HttpGet]
        [Route("reportTypes")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<AvailableReportTypeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAvailableReportTypes()
        {
            try
            {
                var types = await reportService.GetAvailableReportTypes();
                return Ok(types);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["SomethingIsMissing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }
    }
}
