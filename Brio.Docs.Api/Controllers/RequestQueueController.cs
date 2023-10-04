using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Brio.Docs.Client.Services;
using Brio.Docs.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using static Brio.Docs.Api.Validators.ServiceResponsesValidator;

namespace Brio.Docs.Api.Controllers
{
    /// <summary>
    /// Controller for managing long running jobs.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class RequestQueueController : Controller
    {
        private readonly IRequestQueueForApiService service;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public RequestQueueController(IRequestQueueForApiService service, IStringLocalizer<SharedLocalization> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Get progress from 0 to 1 indicating job completion status.
        /// Job is being complete when this method returns 1.
        /// </summary>
        /// <param name="id">Id of the job to check progress on.</param>
        /// <returns>Double value of the current progress.</returns>
        /// <response code="200">Returns double value.</response>
        /// <response code="404">If job with that id is not found.</response>
        /// <response code="500">If something went terribly wrong on the server side.</response>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetProgress(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_JobIdRequired")]
            string id)
        {
            try
            {
                var progress = await service.GetProgress(id);
                return Ok(progress);
            }
            catch (ArgumentException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidJobId_Missing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Get result of the finished job and delete it from the queue.
        /// </summary>
        /// <param name="id">Id of the job.</param>
        /// <returns>Job result.</returns>
        /// <response code="200">Returns result of the completed job.</response>
        /// <response code="404">If job with that id is not found.</response>
        /// <response code="400">If job was not finished yet.</response>
        /// <response code="500">If something went terribly wrong on the server side.</response>
        [HttpGet]
        [Route("result/{id}")]
        public async Task<IActionResult> GetResult(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_JobIdRequired")]
            string id)
        {
            try
            {
                var result = await service.GetResult(id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidJobId_Missing"], ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return CreateProblemResult(this, 400, localizer["JobIsUnfinished"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Cancel requested long running job and delete it from the queue.
        /// </summary>
        /// <param name="id">Id of the job.</param>
        /// <returns>True if canceled successfully.</returns>
        /// <response code="200">Returns if job was canceled successfully.</response>
        /// <response code="404">If job with that id is not found.</response>
        /// <response code="500">If something went terribly wrong on the server side.</response>
        [HttpGet]
        [Route("cancel/{id}")]
        public IActionResult Cancel(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_JobIdRequired")]
            string id)
        {
            try
            {
                service.Cancel(id);
                return Ok(true);
            }
            catch (ArgumentException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidJobId_Missing"], ex.Message);
            }
            catch (Exception ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }
    }
}
