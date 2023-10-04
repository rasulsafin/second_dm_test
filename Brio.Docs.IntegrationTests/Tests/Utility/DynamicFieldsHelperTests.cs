using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Common;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Services;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Brio.Docs.Tests.Utility
{
    [TestClass]
    public class DynamicFieldsHelperTests
    {
        private static ObjectiveService objectiveService;
        private static DynamicFieldsHelper helper;
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

                var users = MockData.DEFAULT_USERS;
                var projects = MockData.DEFAULT_PROJECTS;
                var objectiveTypes = MockData.DEFAULT_OBJECTIVE_TYPES;
                var dynamicFieldInfos = MockData.DEFAULT_DYNAMIC_FIELD_INFOS;
                var enumerationType = MockData.DEFAULT_ENUM_TYPES;

                projects.First().Items = MockData.DEFAULT_ITEMS;
                context.Users.AddRange(users);
                context.Projects.AddRange(projects);
                context.ObjectiveTypes.AddRange(objectiveTypes);
                context.EnumerationTypes.AddRange(enumerationType);
                context.SaveChanges();
            });

            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddHelpers();
            services.AddMappingResolvers();
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddSingleton(x => Fixture.Context);

            serviceProvider = services.BuildServiceProvider();
            mapper = serviceProvider.GetService<IMapper>();
            helper = serviceProvider.GetService<DynamicFieldsHelper>();
            objectiveService = new ObjectiveService(
                Fixture.Context,
                mapper,
                serviceProvider.GetService<ItemsHelper>(),
                helper,
                serviceProvider.GetService<BimElementsHelper>(),
                Mock.Of<ILogger<ObjectiveService>>());
        }

        [TestCleanup]
        public void Cleanup()
        {
            Fixture.Dispose();
            serviceProvider.Dispose();
        }

        [TestMethod]
        [DataRow("keyString", "name", DynamicFieldType.STRING, "value", DisplayName = "DynamicFieldType.STRING")]
        [DataRow("keyBool", "name", DynamicFieldType.BOOL, true, DisplayName = "DynamicFieldType.BOOL")]
        [DataRow("keyInt", "name", DynamicFieldType.INTEGER, 1, DisplayName = "DynamicFieldType.INTEGER")]
        [DataRow("keyFloat", "name", DynamicFieldType.FLOAT, 1f, DisplayName = "DynamicFieldType.FLOAT")]
        public async Task AddDynamicFieldsAsync_ToExistingObjectiveWithDynamicFieldSimple_AddsDynamicFieldToObjective(string key, string name, DynamicFieldType type, object value)
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.DynamicFields = new List<DynamicFieldDto>()
            {
                new DynamicFieldDto()
                {
                    Key = key,
                    Name = name,
                    Type = type,
                    Value = value,
                },
            };
            var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);
            await Fixture.Context.Objectives.AddAsync(objectiveToSave);
            await Fixture.Context.SaveChangesAsync();

            var startCount = Fixture.Context.DynamicFields.Unsynchronized().Count();
            var expectedCount = startCount + 1;

            // Act
            var resultObjective = await helper.AddDynamicFieldsAsync(
               objectiveToCreate.DynamicFields,
               objectiveToSave,
               objectiveToCreate.AuthorID ?? ID<UserDto>.InvalidID);
            var actualCount = Fixture.Context.DynamicFields.Unsynchronized().Count();

            // Assert
            Assert.AreEqual(expectedCount, actualCount);
            Assert.IsNotNull(resultObjective.DynamicFields);
            Assert.AreEqual(1, resultObjective.DynamicFields.Count());
        }

        [TestMethod]
        public async Task AddDynamicFieldsAsync_ToExistingObjectiveWithDynamicFieldEnum_AddsDynamicFieldToObjective()
        {
            // Arrange
            var existingEnumType = Fixture.Context.EnumerationTypes.First();
            var existingEnumValue = existingEnumType.EnumerationValues.First();

            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.DynamicFields = new List<DynamicFieldDto>()
            {
                new DynamicFieldDto()
                {
                    Key = existingEnumType.ID.ToString(),
                    Name = existingEnumType.Name,
                    Type = DynamicFieldType.ENUM,
                    Value = mapper.Map<EnumerationValueDto>(existingEnumValue),
                },
            };
            var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);
            await Fixture.Context.Objectives.AddAsync(objectiveToSave);
            await Fixture.Context.SaveChangesAsync();

            var startCount = Fixture.Context.DynamicFields.Unsynchronized().Count();
            var expectedCount = startCount + 1;

            // Act
            var resultObjective = await helper.AddDynamicFieldsAsync(
                objectiveToCreate.DynamicFields,
                objectiveToSave,
                objectiveToCreate.AuthorID ?? ID<UserDto>.InvalidID);
            var actualCount = Fixture.Context.DynamicFields.Unsynchronized().Count();

            // Assert
            Assert.AreEqual(expectedCount, actualCount);
            Assert.IsNotNull(resultObjective.DynamicFields);
            Assert.AreEqual(1, resultObjective.DynamicFields.Count());
        }

        [TestMethod]
        public async Task AddDynamicFieldsAsync_ToExistingObjectiveWithDynamicFieldDate_AddsDynamicFieldToObjective()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.DynamicFields = new List<DynamicFieldDto>()
            {
                new DynamicFieldDto()
                {
                    Key = "keyDate",
                    Name = "Name",
                    Type = DynamicFieldType.DATE,
                    Value = DateTime.Now,
                },
            };
            var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);
            await Fixture.Context.Objectives.AddAsync(objectiveToSave);
            await Fixture.Context.SaveChangesAsync();

            var startCount = Fixture.Context.DynamicFields.Unsynchronized().Count();
            var expectedCount = startCount + 1;

            // Act
            var resultObjective = await helper.AddDynamicFieldsAsync(
                objectiveToCreate.DynamicFields,
                objectiveToSave,
                objectiveToCreate.AuthorID ?? ID<UserDto>.InvalidID);
            var actualCount = Fixture.Context.DynamicFields.Unsynchronized().Count();

            // Assert
            Assert.AreEqual(expectedCount, actualCount);
            Assert.IsNotNull(resultObjective.DynamicFields);
            Assert.AreEqual(1, resultObjective.DynamicFields.Count());
        }

        [TestMethod]
        public async Task AddDynamicFieldsAsync_ToExistingObjectiveWithDynamicFieldObject_AddsDynamicFieldToObjective()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.DynamicFields = new List<DynamicFieldDto>()
            {
                new DynamicFieldDto()
                {
                    Key = "keyObject",
                    Name = "Name",
                    Type = DynamicFieldType.OBJECT,
                    Value = new List<DynamicFieldDto>()
                    {
                        new DynamicFieldDto()
                        {
                            Key = "keyDate",
                            Name = "Name",
                            Type = DynamicFieldType.DATE,
                            Value = DateTime.Now,
                        },
                        new DynamicFieldDto()
                        {
                            Key = "keyString",
                            Name = "Name",
                            Type = DynamicFieldType.STRING,
                            Value = "text",
                        },
                    },
                },
            };
            var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);
            await Fixture.Context.Objectives.AddAsync(objectiveToSave);
            await Fixture.Context.SaveChangesAsync();

            var startCount = Fixture.Context.DynamicFields.Unsynchronized().Count();
            var expectedCount = startCount + 3;

            // Act
            var resultObjective = await helper.AddDynamicFieldsAsync(
                objectiveToCreate.DynamicFields,
                objectiveToSave,
                objectiveToCreate.AuthorID ?? ID<UserDto>.InvalidID);
            var actualCount = Fixture.Context.DynamicFields.Unsynchronized().Count();

            // Assert
            Assert.AreEqual(expectedCount, actualCount);
            Assert.IsNotNull(resultObjective.DynamicFields);
            Assert.AreEqual(1, resultObjective.DynamicFields.Count());
        }

        [TestMethod]
        public async Task UpdateDynamicFields_ExistingObjectiveDynamicFields_ReturnsTrue()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.DynamicFields = new List<DynamicFieldDto>()
            {
                new DynamicFieldDto()
                {
                    Key = "keyDate",
                    Name = "Name",
                    Type = DynamicFieldType.STRING,
                    Value = "value",
                },
            };
            var objectiveId = (await objectiveService.Add(objectiveToCreate)).ID;
            var existingObjective = await objectiveService.Find(objectiveId);

            // Act
            var dynamicField = existingObjective.DynamicFields.First();
            dynamicField.Value += "edit";
            var result = await helper.UpdateDynamicFieldsAsync(existingObjective.DynamicFields, (int)existingObjective.ID);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task UpdateDynamicFields_ExistingObjectiveDynamicFields_ValuesAreUpdated()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.DynamicFields = new List<DynamicFieldDto>()
            {
                new DynamicFieldDto()
                {
                    Key = "keyDate",
                    Name = "Name",
                    Type = DynamicFieldType.STRING,
                    Value = "value",
                },
            };
            var objectiveId = (await objectiveService.Add(objectiveToCreate)).ID;
            var existingObjective = await objectiveService.Find(objectiveId);

            // Act
            var dynamicField = existingObjective.DynamicFields.First();
            var excpectedValue = dynamicField.Value += "edit";
            await helper.UpdateDynamicFieldsAsync(existingObjective.DynamicFields, (int)existingObjective.ID);
            var updatedObjective = await objectiveService.Find(objectiveId);
            var actualValue = updatedObjective.DynamicFields.First().Value;

            // Assert
            Assert.AreEqual(excpectedValue, actualValue);
        }

        private ObjectiveToCreateDto ArrangeSimpleObjective()
        {
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.Unsynchronized().First();
            var author = Fixture.Context.Users.First();
            return new ObjectiveToCreateDto
            {
                Title = $"Test issue {Guid.NewGuid()}",
                Description = "created for test purpose only",
                Status = ObjectiveStatus.Open,
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(1),
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID),
                AuthorID = new ID<UserDto>(author.ID),
            };
        }

        private bool CompareDynamicFieldDtoToModel(DynamicFieldDto dto, DynamicField model)
        {
            return dto.Name == model.Name
                && dto.Type == (DynamicFieldType)Enum.Parse(typeof(DynamicFieldType), model.Type);
        }
    }
}
