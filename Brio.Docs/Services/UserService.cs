using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Services
{
    public class UserService : IUserService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly CryptographyHelper cryptographyHelper;
        private readonly ILogger<UserService> logger;

        public UserService(
            DMContext context,
            IMapper mapper,
            CryptographyHelper helper,
            ILogger<UserService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            cryptographyHelper = helper;
            this.logger = logger;
            logger.LogTrace("UserService created");
        }

        public virtual async Task<ID<UserDto>> Add(UserToCreateDto user)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with user: {@User}", user);
            try
            {
                if (context.Users.Any(x => x.Login.ToLower() == user.Login.ToLower()))
                    throw new ArgumentValidationException("This login is already being used");

                var newUser = mapper.Map<User>(user);
                logger.LogDebug("Mapped user: {@User}", newUser);
                cryptographyHelper.CreatePasswordHash(user.Password, out byte[] passHash, out byte[] passSalt);
                newUser.PasswordHash = passHash;
                newUser.PasswordSalt = passSalt;
                await context.Users.AddAsync(newUser);
                await context.SaveChangesAsync();

                var userID = new ID<UserDto>(newUser.ID);
                return userID;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't add user {@User}", user);
                if (ex is ArgumentValidationException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public virtual async Task<bool> Delete(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Delete started with userID: {@UserID}", userID);

            try
            {
                var user = await GetUserChecked(userID);
                logger.LogDebug("Found user: {@User}", user);

                context.Users.Remove(user);
                await context.SaveChangesAsync();

                var orphanRoles = await context.Roles
                    .Include(x => x.Users)
                    .Where(x => !x.Users.Any())
                    .ToListAsync();
                logger.LogDebug("Orphan roles: {@OrphanRoles}", orphanRoles);

                if (orphanRoles.Any())
                {
                    context.Roles.RemoveRange(orphanRoles);
                    await context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't delete user {UserID}", userID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<bool> Exists(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Exists started with userID: {UserID}", userID);
            try
            {
                return await context.Users.AnyAsync(x => x.ID == (int)userID);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get user {UserID}", userID);
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<bool> Exists(string login)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Exists started with login: {Login}", login);
            try
            {
                login = login?.Trim();
                return await context.Users.AnyAsync(x => x.Login == login);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get user {Login}", login);
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<UserDto> Find(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started userID: {UserID}", userID);
            try
            {
                var dbUser = await context.Users
                    .Include(x => x.ConnectionInfo)
                        .ThenInclude(c => c.ConnectionType)
                    .FindOrThrowAsync(x => x.ID, (int)userID);
                logger.LogDebug("Found user: {@User}", dbUser);

                return mapper.Map<UserDto>(dbUser);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get user {UserID}", userID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<UserDto> Find(string login)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started with login: {Login}", login);
            try
            {
                login = login?.Trim();
                var dbUser = await context.Users.FindWithIgnoreCaseOrThrowAsync(x => x.Login, login);
                logger.LogDebug("Found user: {@User}", dbUser);

                return mapper.Map<UserDto>(dbUser);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get user {Login}", login);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<UserDto>> GetAllUsers()
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetAllUsers started");
            try
            {
                var dbUsers = await context.Users.ToListAsync();
                logger.LogDebug("Found users: {@Users}", dbUsers);
                return dbUsers.Select(x => mapper.Map<UserDto>(x)).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get list of users");
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<bool> SetCurrent(ID<UserDto> userID)
        {
            if (!await Exists(userID))
                return false;

            CurrentUser.ID = (int)userID;
            return true;
        }

        public virtual async Task<bool> Update(UserDto user)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Update started with user: {@User}", user);

            try
            {
                if (context.Users.Any(x => x.Login.ToLower() == user.Login.ToLower()))
                    throw new ArgumentValidationException("This login is already being used");

                var storedUser = await GetUserChecked(user.ID);
                logger.LogDebug("Found user: {@User}", storedUser);
                storedUser.Login = user.Login;
                storedUser.Name = user.Name;
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't update user {@User}", user);
                if (ex is ArgumentValidationException || ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public virtual async Task<bool> UpdatePassword(ID<UserDto> userID, string newPass)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("UpdatePassword started with userID: {@UserID}", userID);
            try
            {
                var user = await GetUserChecked(userID);
                logger.LogDebug("Found user: {@User}", user);
                cryptographyHelper.CreatePasswordHash(newPass, out byte[] passHash, out byte[] passSalt);
                user.PasswordHash = passHash;
                user.PasswordSalt = passSalt;
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't update password of user {UserID}", userID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<bool> VerifyPassword(ID<UserDto> userID, string password)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("VerifyPassword started with userID: {@UserID}", userID);
            try
            {
                var user = await GetUserChecked(userID);
                logger.LogDebug("Found user: {@User}", user);
                var result = cryptographyHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt);

                if (!result)
                    throw new ArgumentValidationException("Wrong password!");

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't verify password of user {UserID}", userID);
                if (ex is ArgumentValidationException || ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        private async Task<User> GetUserChecked(ID<UserDto> userID)
        {
            logger.LogTrace("GetUserChecked started with userID: {UserID}", userID);
            var user = await context.Users.FindOrThrowAsync((int)userID);
            logger.LogDebug("Found user: {@User}", user);
            return user;
        }
    }
}
