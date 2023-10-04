using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Brio.Docs.Api.Validators;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services;
using Brio.Docs.Client.Services.ForApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using static Brio.Docs.Api.Validators.ServiceResponsesValidator;

namespace Brio.Docs.Api.Controllers
{
    /// TODO: Set rules for login and password (min-max length, permitted symbols etc)
    /// <summary>
    /// Controller for managing User entities.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserForApiService service;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public UsersController(IUserForApiService service,
            IStringLocalizer<SharedLocalization> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Get list of all existing users.
        /// </summary>
        /// <returns>List of users.</returns>
        /// <response code="200">Returns found list of users.</response>
        /// <response code="500">Something went wrong while retrieving the users.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await service.GetAllUsers();
                return Ok(users);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Create new user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Users
        ///     {
        ///        "login": "loginValue",
        ///        "password": "passwordValue",
        ///        "name": "nameValue"
        ///     }
        /// </remarks>
        /// <param name="data">User data.</param>
        /// <returns>Id of the created user.</returns>
        /// <response code="201">Returns created user id.</response>
        /// <response code="400">User with the same login already exists OR one/multiple of required values is/are empty.</response>
        /// <response code="500">Something went wrong while creating new user.</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add(
            [FromBody]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Add")]
            UserToCreateDto data)
        {
            try
            {
                var userId = await service.Add(data);
                return Created(string.Empty, userId);
            }
            catch (ArgumentValidationException ex)
            {
                return CreateProblemResult(this, 400, localizer["CheckValidUserToCreate_AlreadyExists"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Add"], ex.Message);
            }
        }

        /// <summary>
        /// Delete existing user.
        /// </summary>
        /// <param name="userID">Id of the user to be deleted.</param>
        /// <returns>True if user is deleted.</returns>
        /// <response code="200">User was deleted successfully.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">User was not found.</response>
        /// <response code="500">Something went wrong while deleting user.</response>
        [HttpDelete]
        [Route("{userID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID)
        {
            try
            {
                await service.Delete(new ID<UserDto>(userID));
                return Ok(true);
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

        /// <summary>
        /// Update user's values to given data.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /Users
        ///     {
        ///        "id": { "id" : 0 },
        ///        "login": "loginValue",
        ///        "name": "nameValue",
        ///        "role": null
        ///     }
        /// </remarks>
        /// <param name="user">UserDto object.</param>
        /// <returns>True, if updated successfully.</returns>
        /// <response code="200">User was updated successfully.</response>
        /// <response code="400">Some of user's data is null.</response>
        /// <response code="404">Could not find user to update.</response>
        /// <response code="500">Something went wrong while updating user.</response>
        [HttpPut]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(
            [FromBody]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Put")]
            UserDto user)
        {
            try
            {
                await service.Update(user);
                return Ok(true);
            }
            catch (ArgumentValidationException ex)
            {
                return CreateProblemResult(this, 400, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserToCreate_AlreadyExists"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Put"], ex.Message);
            }
        }

        /// <summary>
        ///  Verify password.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <param name="password">Password to verify.</param>
        /// <returns>True if password verified.</returns>
        /// <response code="200">Password was verified.</response>
        /// <response code="400">Wrong password.</response>
        /// <response code="404">Could not find user to verify password.</response>
        /// <response code="500">Something went wrong while verifying.</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Route("{userID}/password")]
        public async Task<IActionResult> VerifyPassword(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID,
            [FromBody]
            [Required(ErrorMessage = "ValidationError_PasswordIsRequired")]
            string password)
        {
            try
            {
                 await service.VerifyPassword(new ID<UserDto>(userID), password);
                 return Ok(true);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (ArgumentValidationException ex)
            {
                return CreateProblemResult(this, 400, localizer["WrongPassword"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError"], ex.Message);
            }
        }

        /// <summary>
        ///  Update password.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <param name="newPass">New password.</param>
        /// <returns>True if password updated.</returns>
        /// <response code="200">Password updated.</response>
        /// <response code="400">Something is null.</response>
        /// <response code="404">Could not find user to update password.</response>
        /// <response code="500">Something went wrong while updating.</response>
        [HttpPut]
        [Route("{userID}/password")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePassword(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID,
            [FromBody]
            [Required(ErrorMessage = "ValidationError_PasswordIsRequired")]
            string newPass)
        {
            try
            {
                await service.UpdatePassword(new ID<UserDto>(userID), newPass);
                return Ok(true);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Put"], ex.Message);
            }
        }

        /// <summary>
        /// Get user by their id.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <returns>Found user.</returns>
        /// <response code="200">User found.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Could not find user.</response>
        /// <response code="500">Something went wrong while retrieving the user.</response>
        [HttpGet]
        [Route("{userID}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID)
        {
            try
            {
                var foundUser = await service.Find(userID);
                return Ok(foundUser);
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
        /// Get user by their login.
        /// </summary>
        /// <param name="login">User's login.</param>
        /// <returns>Found user.</returns>
        /// <response code="200">User found.</response>
        /// <response code="400">Invalid login.</response>
        /// <response code="404">Could not find user.</response>
        /// <response code="500">Something went wrong while retrieving the user.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("find")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find(
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_LoginIsRequired")]
            string login)
        {
            try
            {
                var foundUser = await service.Find(login);
                return Ok(foundUser);
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
        /// Check if user exists by id.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <returns>True if user exists, false otherwise.</returns>
        /// <response code="200">User exists.</response>
        /// <response code="400">If id is invalid.</response>
        /// <response code="500">Something went wrong while checking the user.</response>
        [HttpGet]
        [Route("exists/{userID}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Exists(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID)
        {
            try
            {
                var exists = await service.Exists(userID);
                return Ok(exists);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Check if user exists by login.
        /// </summary>
        /// <param name="login">User's login.</param>
        /// <returns>True if user exists, false otherwise.</returns>
        /// <response code="200">User exists.</response>
        /// <response code="400">Invalid login.</response>
        /// <response code="500">Something went wrong while checking the user.</response>
        [HttpGet]
        [Route("exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Exists(
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_LoginIsRequired")]
            string login)
        {
            try
            {
                var exists = await service.Exists(login);
                return Ok(exists);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// TEMPORARY: Set current user if user exists.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <returns>Code 200 if everything is fine.</returns>
        /// <response code="200">User exists.</response>
        /// <response code="400">If id is invalid.</response>
        /// <response code="500">Something went wrong while checking the user.</response>
        [HttpPost]
        [Route("current")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetCurrent(
            [FromQuery]
            int userID)
        {
            try
                {
                var result = await service.SetCurrent(userID);
                return Ok(result);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }
    }
}
