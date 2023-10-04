using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Database.Models;
using Brio.Docs.Services;
using Brio.Docs.Tests.Utility;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Brio.Docs.Tests.Services
{
    [TestClass]
    public class AuthorizationTests
    {
        private static AuthorizationService service;
        private static IMapper mapper;

        private static SharedDatabaseFixture Fixture { get; set; }

        [ClassInitialize]
        public static void ClassSetup(TestContext unused)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            mapper = mapperConfig.CreateMapper();
        }

        [TestInitialize]
        public void Setup()
        {
            Fixture = new SharedDatabaseFixture(context =>
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var users = MockData.DEFAULT_USERS;
                var roles = MockData.DEFAULT_ROLES;
                context.Users.AddRange(users);
                context.Roles.AddRange(roles);
                context.SaveChanges();

                if (users.Count >= 3 && roles.Count >= 2)
                {
                    var userRoles = new List<UserRole>
                    {
                        new UserRole { UserID = users[0].ID, RoleID = roles[0].ID },
                        new UserRole { UserID = users[1].ID, RoleID = roles[0].ID },
                        new UserRole { UserID = users[2].ID, RoleID = roles[1].ID },
                    };
                    context.UserRoles.AddRange(userRoles);
                    context.SaveChanges();
                }
            });

            service = new AuthorizationService(
                Fixture.Context,
                mapper,
                new CryptographyHelper(),
                Mock.Of<ILogger<AuthorizationService>>());
        }

        [TestCleanup]
        public void Cleanup() => Fixture.Dispose();

        [TestMethod]
        public async Task AddRole_ExistingUserAndExistingRole_ReturnsTrueWithoutAddingRole()
        {
            var existingUser = await Fixture.Context.Users.FirstAsync(u => u.Roles.Count == 1);
            var userId = existingUser.ID;
            var currentRole = existingUser.Roles.First().Role;
            var roleToAdd = Fixture.Context.Roles.First(r => r != currentRole);
            var rolesCount = Fixture.Context.Roles.Count();

            var result = await service.AddRole(new ID<UserDto>(userId), roleToAdd.Name);

            Assert.IsTrue(result);
            Assert.IsTrue(existingUser.Roles.Any(r => r.Role == roleToAdd));
            Assert.AreEqual(rolesCount, Fixture.Context.Roles.Count());
        }

        [TestMethod]
        public async Task AddRole_ExistingUserAndNotExistingRole_ReturnsTrueAndAddsRole()
        {
            var existingUser = await Fixture.Context.Users.FirstAsync(u => u.Roles.Count == 1);
            var userId = existingUser.ID;
            var roleToAdd = $"newRole{Guid.NewGuid()}";
            var rolesCount = Fixture.Context.Roles.Count();

            var result = await service.AddRole(new ID<UserDto>(userId), roleToAdd);

            Assert.IsTrue(result);
            Assert.IsTrue(existingUser.Roles.Any(r => r.Role.Name == roleToAdd));
            Assert.AreEqual(rolesCount + 1, Fixture.Context.Roles.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<User>))]
        public async Task AddRole_NotExistingUser_RaisesNotFoundException()
        {
            var userId = ID<UserDto>.InvalidID;
            var role = "admin";

            await service.AddRole(userId, role);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentValidationException))]
        public async Task AddRole_UserAlreadyInRole_RaisesArgumentException()
        {
            var existingUser = await Fixture.Context.Users.FirstAsync(u => u.Roles.Count > 0);
            var usersId = new ID<UserDto>(existingUser.ID);
            var usersRole = existingUser.Roles.First().Role.Name;

            await service.AddRole(usersId, usersRole);

            Assert.Fail();
        }

        [TestMethod]
        public async Task GetAllRoles_NormalWay_ReturnsAllRolesNames()
        {
            var contextRoles = Fixture.Context.Roles.Select(r => r.Name).ToList();

            var result = await service.GetAllRoles();

            contextRoles.ForEach(r =>
            {
                Assert.IsTrue(result.Any(resRole => resRole.Equals(r)));
            });
        }

        [TestMethod]
        public async Task GetUserRoles_ExistingUser_ReturnsAllUsersRolesNames()
        {
            var existingUser = Fixture.Context.Users.First(u => u.Roles.Count > 0);
            var existingUserRolesNames = existingUser.Roles.Select(r => r.Role.Name).ToList();

            var result = await service.GetUserRoles(new ID<UserDto>(existingUser.ID));

            existingUserRolesNames.ForEach(r =>
            {
                Assert.IsTrue(result.Any(resRole => resRole.Equals(r)));
            });
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<User>))]
        public async Task GetUserRoles_NotExistingUser_RaisesNotFoundException()
        {
            await service.GetUserRoles(ID<UserDto>.InvalidID);

            Assert.Fail();
        }

        [TestMethod]
        public async Task IsInRole_ExistingUserInTheRole_ReturnsTrue()
        {
            var existingUser = Fixture.Context.Users.First(u => u.Roles.Count > 0);
            var userRole = existingUser.Roles.First().Role.Name;

            var result = await service.IsInRole(new ID<UserDto>(existingUser.ID), userRole);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsInRole_ExistingUserNotInTheRole_ReturnsFalse()
        {
            var existingUser = Fixture.Context.Users.First(u => u.Roles.Count == 1);
            var userRole = existingUser.Roles.First().Role;
            var notUserRole = Fixture.Context.Roles.First(r => r != userRole);

            var result = await service.IsInRole(new ID<UserDto>(existingUser.ID), notUserRole.Name);

            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<User>))]
        public async Task IsInRole_NotExistingUser_RaisesNotFoundException()
        {
            var role = Fixture.Context.Roles.First();

            await service.IsInRole(ID<UserDto>.InvalidID, role.Name);

            Assert.Fail();
        }

        [TestMethod]
        public async Task RemoveRole_RoleWithExistingSingleUser_ReturnsTrueAndRemovesRoleWithEmptyUsersList()
        {
            var singleUserRole = Fixture.Context.UserRoles.First(r => r.Role.Users.Count == 1).Role;
            var user = singleUserRole.Users.First().User;

            var result = await service.RemoveRole(new ID<UserDto>(user.ID), singleUserRole.Name);

            Assert.IsTrue(result);
            Assert.IsFalse(user.Roles.Any(r => r.Role == singleUserRole));
            Assert.IsFalse(Fixture.Context.Roles.Any(r => r == singleUserRole));
            Assert.IsFalse(Fixture.Context.UserRoles.Any(ur => ur.Role == singleUserRole));
        }

        [TestMethod]
        public async Task RemoveRole_RoleWithExistingMultipleUser_ReturnsTrueAndDoesntRemoveRole()
        {
            var multipleUsersRole = Fixture.Context.UserRoles.First(r => r.Role.Users.Count > 1).Role;
            var user = multipleUsersRole.Users.First().User;

            var result = await service.RemoveRole(new ID<UserDto>(user.ID), multipleUsersRole.Name);

            Assert.IsTrue(result);
            Assert.IsFalse(user.Roles.Any(r => r.Role == multipleUsersRole));
            Assert.IsTrue(Fixture.Context.Roles.Contains(multipleUsersRole));
            Assert.IsTrue(Fixture.Context.UserRoles.Any(ur => ur.Role == multipleUsersRole));
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<User>))]
        public async Task RemoveRole_NotExistingUser_RaisesNotFoundException()
        {
            var role = Fixture.Context.Roles.First();

            await service.RemoveRole(ID<UserDto>.InvalidID, role.Name);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentValidationException))]
        public async Task RemoveRole_ExistingUserWithoutRole_RaisesArgumentException()
        {
            var existingUser = Fixture.Context.Users.First(u => u.Roles.Count == 1);
            var userRole = existingUser.Roles.First().Role;
            var notUserRole = Fixture.Context.Roles.First(r => r != userRole);

            await service.RemoveRole(new ID<UserDto>(existingUser.ID), notUserRole.Name);

            Assert.Fail();
        }

        [TestMethod]
        public async Task Login_ExistingUserWithCorrectPasswordAndWithRole_ReturnsValidatedUserWithRoles()
        {
            var user = Fixture.Context.Users.First(u => u.Roles.Any());
            var username = user.Login;
            var password = "pass";
            var userPassHash = user.PasswordHash;
            var userPassSalt = user.PasswordSalt;
            var mockedCryptographyHelper = new Mock<CryptographyHelper>();
            mockedCryptographyHelper.Setup(m => m.VerifyPasswordHash(password, userPassHash, userPassSalt)).Returns(true);
            var mockedService = new AuthorizationService(
                Fixture.Context,
                mapper,
                mockedCryptographyHelper.Object,
                Mock.Of<ILogger<AuthorizationService>>());

            var result = await mockedService.Login(username, password);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValidationSuccessful);
            Assert.IsTrue(result.User.Login == username);
            Assert.AreEqual(user.Roles.First().Role.Name, result.User.Role.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<User>))]
        public async Task Login_NotExistingUser_RaisesNotFoundException()
        {
            var username = $"notExistingLogin{Guid.NewGuid()}";
            var password = "pass";

            await service.Login(username, password);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentValidationException))]
        public async Task Login_ExistingUserWithInvalidPassword_RaisesArgumentException()
        {
            var user = Fixture.Context.Users.First(u => u.Roles.Any());
            var username = user.Login;
            var password = "pass";
            var userPassHash = user.PasswordHash;
            var userPassSalt = user.PasswordSalt;
            var mockedCryptographyHelper = new Mock<CryptographyHelper>();
            mockedCryptographyHelper.Setup(m => m.VerifyPasswordHash(password, userPassHash, userPassSalt)).Returns(false);
            var mockedService = new AuthorizationService(
                Fixture.Context,
                mapper,
                mockedCryptographyHelper.Object,
                Mock.Of<ILogger<AuthorizationService>>());

            await mockedService.Login(username, password);

            Assert.Fail();
        }
    }
}
