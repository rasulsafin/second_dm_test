using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
using Microsoft.IdentityModel.Tokens;
using static Brio.Docs.Api.Validators.ServiceResponsesValidator;

namespace Brio.Docs.Api.Controllers
{
    /// <summary>
    /// Controller for managing authorization of users.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class AuthorizationsController : ControllerBase
    {
        private readonly IAuthorizationForApiService service;
        private readonly IStringLocalizer<SharedLocalization> localizer;

        public AuthorizationsController(
            IAuthorizationForApiService service,
            IStringLocalizer<SharedLocalization> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Get all registered roles.
        /// </summary>
        /// <returns>Empty enumerable if no roles were registered.</returns>
        /// <response code="200">Collection of roles.</response>
        /// <response code="500">Something went wrong while trying to get list of roles.</response>
        [HttpGet]
        [Route("roles")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await service.GetAllRoles();
                return Ok(roles);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        /// <summary>
        /// Add role to user.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <param name="role">Role to add.</param>
        /// <returns>True if role is added.</returns>
        /// <response code="200">True if role is added to user successfully.</response>
        /// <response code="400">Invalid project id, role OR user had this role already.</response>
        /// <response code="404">Could not find user to apply role to.</response>
        /// <response code="500">Something went wrong while trying to add role to user.</response>
        [HttpPost]
        [Route("user/roles")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddRole(
            [FromQuery]
            [CheckValidID]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            int userID,
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_ObjectRequired_Add")]
            string role)
        {
            try
            {
                var added = await service.AddRole(new ID<UserDto>(userID), role);
                return Ok(added);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (ArgumentValidationException ex)
            {
                return CreateProblemResult(this, 400, localizer["UserHasRoleAlready"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Add"], ex.Message);
            }
        }

        /// <summary>
        /// Remove role from user. If this role is not referenced anymore it's deleted.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <param name="role">Role to remove.</param>
        /// <returns>True if role was removed.</returns>
        /// <response code="200">Role was deleted from user successfully.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">User was not found.</response>
        /// <response code="500">Something went wrong while deleting role from the user.</response>
        [HttpDelete]
        [Route("user/roles")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveRole(
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID,
            [FromQuery]
            string role)
        {
            try
            {
                await service.RemoveRole(new ID<UserDto>(userID), role);
                return Ok(true);
            }
            catch (ANotFoundException ex)
            {
                return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], ex.Message);
            }
            catch (ArgumentValidationException ex)
            {
                return CreateProblemResult(this, 400, localizer["UserHasNoRoleAlready"], ex.Message);
            }
            catch (DocumentManagementException ex)
            {
                return CreateProblemResult(this, 500, localizer["ServerError_Delete"], ex.Message);
            }
        }

        /// <summary>
        /// Get all roles of specified user.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <returns>Collection of roles.</returns>
        /// <response code="200">Collection of user's roles.</response>
        /// <response code="400">Invalid user id.</response>
        /// <response code="404">User was not found.</response>
        /// <response code="500">Something went wrong while retrieving roles from the user.</response>
        [HttpGet]
        [Route("user/{userID}/roles")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserRoles(
            [FromRoute]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID)
        {
            try
            {
                var roles = await service.GetUserRoles(new ID<UserDto>(userID));
                return Ok(roles);
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
        /// Check if user is in role.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <param name="role">Role to check.</param>
        /// <returns>True if user is in role, false otherwise.</returns>
        /// <response code="200">True or false.</response>
        /// <response code="400">Invalid user id.</response>
        /// <response code="404">User was not found.</response>
        /// <response code="500">Something went wrong while checking user's role.</response>
        [HttpGet]
        [Route("user/roles")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IsInRole(
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            [CheckValidID]
            int userID,
            [FromQuery]
            string role)
        {
            try
            {
                var isInRole = await service.IsInRole(new ID<UserDto>(userID), role);
                return Ok(isInRole);
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
        /// Login the user if credentials are correct.
        /// </summary>
        /// <param name="username">User's login.</param>
        /// <param name="password">User's password.</param>
        /// <returns>Validated user.</returns>
        /// <response code="200">Validated user's data.</response>
        /// <response code="400">Invalid user login OR password is wrong.</response>
        /// <response code="404">User was not found.</response>
        /// <response code="500">Something went wrong while trying to login.</response>
        [HttpPost]
        [Route("login")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(
            [FromQuery]
            [Required(ErrorMessage = "ValidationError_IdIsRequired")]
            string username,
            [FromBody]
            [Required(ErrorMessage = "ValidationError_PasswordIsRequired")]
            string password)
        {
            try
            {
                var identityTuple = await GetIdentity(username, password);
                var identity = identityTuple.Item1;
                var user = identityTuple.Item2;

                if (identity == null)
                    return CreateProblemResult(this, 404, localizer["CheckValidUserID_Missing"], string.Empty);

                var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: identity.Claims,
                    notBefore: DateTime.UtcNow,
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256Signature));

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                var result = (encodedJwt, user);

                return Ok(result);
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
                return CreateProblemResult(this, 500, localizer["ServerError_Get"], ex.Message);
            }
        }

        private async Task<(ClaimsIdentity, ValidatedUserDto)> GetIdentity(string login, string password)
        {
            try
            {
                var validatedUser = await service.Login(login, password);
                if (validatedUser != null && validatedUser.IsValidationSuccessful)
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, validatedUser.User.Login),
                };

                    if (validatedUser.User.Role != null)
                        claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, validatedUser.User.Role.Name));

                    var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

                    return (claimsIdentity, validatedUser);
                }

                return (null, validatedUser);
            }
            catch
            {
                throw;
            }
        }
    }
}
