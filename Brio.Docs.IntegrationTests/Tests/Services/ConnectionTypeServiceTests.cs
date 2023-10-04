using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services;
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
    public class ConnectionTypeServiceTests
    {
        private static IConnectionTypeService service;
        private static IMapper mapper;
        private static ServiceProvider serviceProvider;

        private static SharedDatabaseFixture Fixture { get; set; }

        [TestInitialize]
        public void Setup()
        {
            Fixture = new SharedDatabaseFixture(context =>
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var types = MockData.DEFAULT_CONNECTION_TYPES;
                context.ConnectionTypes.AddRange(types);
                context.SaveChanges();
            });

            IServiceCollection services = new ServiceCollection();

            var mock = new Mock<CryptographyHelper>();
            services.AddSingleton(Fixture.Context);
            services.AddLogging();
            services.AddMappingResolvers();
            services.AddTransient(sp => mock.Object);
            services.AddAutoMapper(typeof(MappingProfile));
            serviceProvider = services.BuildServiceProvider();
            mapper = serviceProvider.GetService<IMapper>();

            service = new ConnectionTypeService(Fixture.Context, mapper, Mock.Of<ILogger<ConnectionTypeService>>());
        }

        [TestCleanup]
        public void Cleanup()
        {
            Fixture.Dispose();
            serviceProvider.Dispose();
        }

        [TestMethod]
        public async Task Add_NewType_ReturnsAddedTypeId()
        {
            var newTypeName = $"newTypeName{Guid.NewGuid()}";

            var result = await service.Add(newTypeName);

            var addedType = Fixture.Context.ConnectionTypes.FirstOrDefault(t => t.Name == newTypeName);
            Assert.IsNotNull(addedType);
            Assert.AreEqual(addedType.ID, (int)result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentValidationException))]
        public async Task Add_ExistingType_RaisesArgumentException()
        {
            var existingType = Fixture.Context.ConnectionTypes.First();

            await service.Add(existingType.Name);

            Assert.Fail();
        }

        [TestMethod]
        public async Task FindById_ExistingType_ReturnsConnectionType()
        {
            var existingType = Fixture.Context.ConnectionTypes.First();

            var result = await service.Find(new ID<ConnectionTypeDto>(existingType.ID));

            Assert.AreEqual(existingType.ID, (int)result.ID);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<ConnectionType>))]
        public async Task FindById_NotExistingType_RaisesNotFoundException()
        {
            var result = await service.Find(ID<ConnectionTypeDto>.InvalidID);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task FindByName_ExistingType_ReturnsConnectionType()
        {
            var existingType = Fixture.Context.ConnectionTypes.First();

            var result = await service.Find(existingType.Name);

            Assert.AreEqual(existingType.Name, result.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<ConnectionType>))]
        public async Task FindByName_NotExistingType_RaisesNotFoundException()
        {
            var notExistingTypeName = $"invalidName{Guid.NewGuid()}";

            await service.Find(notExistingTypeName);

            Assert.Fail();
        }

        [TestMethod]
        public async Task GetAllConnectionTypes_NormalWay_ReturnsConnectionTypesEnumerable()
        {
            var existingType = Fixture.Context.ConnectionTypes.First();

            var result = await service.Find(existingType.Name);

            Assert.AreEqual(existingType.Name, result.Name);
        }

        [TestMethod]
        public async Task Remove_ExistingType_ReturnsTrueAndRemovesType()
        {
            var existingType = Fixture.Context.ConnectionTypes.First();

            var result = await service.Remove(new ID<ConnectionTypeDto>(existingType.ID));

            Assert.IsTrue(result);
            Assert.IsFalse(Fixture.Context.ConnectionTypes.Any(t => t.ID == existingType.ID));
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<ConnectionType>))]
        public async Task Remove_NotExistingType_ReturnsFalse()
        {
            await service.Remove(ID<ConnectionTypeDto>.InvalidID);

            Assert.Fail();
        }
    }
}
