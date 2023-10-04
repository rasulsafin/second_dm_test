using System;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Brio.Docs.Tests.Services
{
    [TestClass]
    public class ObjectiveTypeServiceTests
    {
        private static ObjectiveTypeService service;
        private static IMapper mapper;

        private ServiceProvider serviceProvider;

        private static SharedDatabaseFixture Fixture { get; set; }

        [TestInitialize]
        public void Setup()
        {
            Fixture = new SharedDatabaseFixture(context =>
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.Users.AddRange(MockData.DEFAULT_USERS);
                context.ObjectiveTypes.AddRange(MockData.DEFAULT_OBJECTIVE_TYPES);
                context.SaveChanges();
            });

            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddMappingResolvers();
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddSingleton(x => Fixture.Context);
            serviceProvider = services.BuildServiceProvider();
            mapper = serviceProvider.GetService<IMapper>();

            service = new ObjectiveTypeService(
                Fixture.Context,
                mapper,
                Mock.Of<ILogger<ObjectiveTypeService>>());
            var userService = new UserService(
             Fixture.Context,
             mapper,
             new CryptographyHelper(),
             Mock.Of<ILogger<UserService>>());

            var userDtoId = mapper.Map<ID<UserDto>>(Fixture.Context.Users.First().ID);
            var setCurrentUserTask = userService.SetCurrent(userDtoId);
            setCurrentUserTask.Wait();
            var result = setCurrentUserTask.Result;
            if (!result)
                Assert.Fail("Authorization failed");
        }

        [TestCleanup]
        public void Cleanup()
            => Fixture.Dispose();

        [TestMethod]
        public async Task Add_NewType_ReturnsAddedTypeId()
        {
            var newTypeName = $"newTypeName{Guid.NewGuid()}";

            var result = await service.Add(newTypeName);

            var addedType = Fixture.Context.ObjectiveTypes.FirstOrDefault(t => t.Name == newTypeName);
            Assert.IsNotNull(addedType);
            Assert.AreEqual(addedType.ID, (int)result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentValidationException))]
        public async Task Add_ExistingType_RaisesArgumentException()
        {
            var existingType = Fixture.Context.ObjectiveTypes.First();

            await service.Add(existingType.Name);

            Assert.Fail();
        }

        [TestMethod]
        public async Task FindById_ExistingType_ReturnsObjectiveType()
        {
            var existingType = Fixture.Context.ObjectiveTypes.First();

            var result = await service.Find(new ID<ObjectiveTypeDto>(existingType.ID));

            Assert.AreEqual(existingType.ID, (int)result.ID);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<ObjectiveType>))]
        public async Task FindById_NotExistingType_ReturnsNull()
        {
            await service.Find(ID<ObjectiveTypeDto>.InvalidID);

            Assert.Fail();
        }

        [TestMethod]
        public async Task FindByName_ExistingType_ReturnsObjectiveType()
        {
            var existingType = Fixture.Context.ObjectiveTypes.First();

            var result = await service.Find(existingType.Name);

            Assert.AreEqual(existingType.Name, result.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<ObjectiveType>))]
        public async Task FindByName_NotExistingType_RaisesNotFoundException()
        {
            var notExistingTypeName = $"invalidName{Guid.NewGuid()}";

            await service.Find(notExistingTypeName);

            Assert.Fail();
        }

        [TestMethod]
        public async Task GetAllObjectiveTypes_NormalWay_ReturnsObjectiveTypesEnumerable()
        {
            var existingType = Fixture.Context.ObjectiveTypes.First();

            var result = await service.Find(existingType.Name);

            Assert.AreEqual(existingType.Name, result.Name);
        }

        [TestMethod]
        public async Task Remove_ExistingType_ReturnsTrueAndRemovesType()
        {
            var existingType = Fixture.Context.ObjectiveTypes.First();

            var result = await service.Remove(new ID<ObjectiveTypeDto>(existingType.ID));

            Assert.IsTrue(result);
            Assert.IsFalse(Fixture.Context.ObjectiveTypes.Any(t => t.ID == existingType.ID));
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<ObjectiveType>))]
        public async Task Remove_NotExistingType_RaisesNotFoundException()
        {
            var result = await service.Remove(ID<ObjectiveTypeDto>.InvalidID);

            Assert.Fail();
        }
    }
}
