using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Filters;
using Brio.Docs.Client.Sorts;
using Brio.Docs.Common;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Services;
using Brio.Docs.Tests.Utility;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Brio.Docs.Tests.Services
{
    [TestClass]
    public class ObjectiveServiceTests
    {
        private static ObjectiveService service;
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

                projects.First().Items = MockData.DEFAULT_ITEMS;
                context.Users.AddRange(users);
                context.Projects.AddRange(projects);
                context.ObjectiveTypes.AddRange(objectiveTypes);
                context.SaveChanges();
            });

            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped<DynamicFieldsHelper>();
            services.AddScoped<ItemsHelper>();
            services.AddScoped<BimElementsHelper>();
            services.AddMappingResolvers();
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddSingleton(x => Fixture.Context);

            serviceProvider = services.BuildServiceProvider();
            mapper = serviceProvider.GetService<IMapper>();
            service = new ObjectiveService(
                Fixture.Context,
                mapper,
                serviceProvider.GetService<ItemsHelper>(),
                serviceProvider.GetService<DynamicFieldsHelper>(),
                serviceProvider.GetService<BimElementsHelper>(),
                Mock.Of<ILogger<ObjectiveService>>());

            CurrentUser.ID = Fixture.Context.Users.First().ID;
        }

        [TestCleanup]
        public void Cleanup()
        {
            Fixture.Dispose();
            serviceProvider.Dispose();
        }

        #region ADD

        [TestMethod]
        public async Task Add_NewObjectiveWithSimpleFields_ReturnsRightObjectiveToList()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();

            // Act
            var resultObjective = await service.Add(objectiveToCreate);

            // Assert
            Assert.AreEqual(objectiveToCreate.Title, resultObjective.Title);
            Assert.AreEqual(objectiveToCreate.Description, resultObjective.Description);
            Assert.AreEqual(objectiveToCreate.Status, resultObjective.Status);
            Assert.IsTrue(resultObjective.ID.IsValid);
        }

        [TestMethod]
        public async Task Add_NewObjectiveWithSimpleFields_SavesRightObjectiveToDatabase()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            var startCount = Fixture.Context.Objectives.Unsynchronized().Count();
            var expectedCount = startCount + 1;

            // Act
            var objective = await service.Add(objectiveToCreate);
            var resultObjective = await Fixture.Context.Objectives.FindAsync((int)objective.ID);
            var actualCount = Fixture.Context.Objectives.Unsynchronized().Count();

            // Assert
            Assert.AreEqual(expectedCount, actualCount);
            Assert.IsNotNull(resultObjective);
            Assert.AreEqual(objectiveToCreate.DueDate, resultObjective.DueDate);
            Assert.AreEqual(objectiveToCreate.CreationDate, resultObjective.CreationDate);
        }


        [TestMethod]
        public async Task AddLocationAsync_ToExistingObjectiveWithLocationExistingItem_AddsLocationToObjective()
        {
            // Arrange
            var existingItem = Fixture.Context.Items.Unsynchronized().First(x => x.ItemType == (int)ItemType.Bim);
            var expectedCount = Fixture.Context.Items.Count();

            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.Location = new LocationDto()
            {
                Guid = Guid.NewGuid().ToString(),
                Position = (0, 0, 0),
                CameraPosition = (1, 1, 1),
                Item = mapper.Map<ItemDto>(existingItem),
            };
            var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);
            await Fixture.Context.Objectives.AddAsync(objectiveToSave);
            await Fixture.Context.SaveChangesAsync();

            // Act
            var resultObjective = await service.AddLocationAsync(objectiveToCreate.Location, objectiveToSave);
            var count = Fixture.Context.Items.Count();

            // Assert
            Assert.IsNotNull(resultObjective);
            Assert.AreEqual(expectedCount, count);
            Assert.IsNotNull(resultObjective.Location);
            Assert.AreEqual(existingItem.ID, resultObjective.Location.ItemID);
        }

        [TestMethod]
        public async Task AddLocationAsync_ToExistingObjectiveWithLocationNewItem_AddsLocationToObjectiveAndItemToDb()
        {
            // Arrange
            var expectedCount = Fixture.Context.Items.Count() + 1;

            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.Location = new LocationDto()
            {
                Guid = Guid.NewGuid().ToString(),
                Position = (0, 0, 0),
                CameraPosition = (1, 1, 1),
                Item = new ItemDto()
                {
                    RelativePath = "/path/file.ifc",
                    ItemType = ItemType.Bim,
                },
            };
            var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);
            await Fixture.Context.Objectives.AddAsync(objectiveToSave);
            await Fixture.Context.SaveChangesAsync();

            // Act
            var resultObjective = await service.AddLocationAsync(objectiveToCreate.Location, objectiveToSave);
            var actualCount = Fixture.Context.Items.Count();

            // Assert
            Assert.IsNotNull(resultObjective);
            Assert.AreEqual(expectedCount, actualCount);
            Assert.IsNotNull(resultObjective.Location);
        }

        [TestMethod]
        public async Task Add_NewObjectiveWithExistingAuthor_AddsAuthorToObjective()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();

            // Act
            var objective = await service.Add(objectiveToCreate);
            var resultObjective = await Fixture.Context.Objectives.FindAsync((int)objective.ID);

            // Assert
            Assert.AreEqual((int?)objectiveToCreate.AuthorID, resultObjective.AuthorID);
            Assert.IsNotNull(resultObjective.AuthorID);
        }

        [TestMethod]
        [ExpectedException(typeof(DocumentManagementException))]
        public async Task Add_NewObjectiveWithNewAuthor_ThrowsException()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.AuthorID = new ID<UserDto>();

            // Act
            await service.Add(objectiveToCreate);

            // Assert
            Assert.Fail();
        }

        [TestMethod]
        public async Task Add_NewObjectiveWithExistingType_AddsTypeToObjective()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();

            // Act
            var objective = await service.Add(objectiveToCreate);
            var resultObjective = await Fixture.Context.Objectives.FindAsync((int)objective.ID);

            // Assert
            Assert.AreEqual((int)objectiveToCreate.ObjectiveTypeID, resultObjective.ObjectiveTypeID);
            Assert.IsNotNull(resultObjective.ObjectiveTypeID);
        }

        [TestMethod]
        [ExpectedException(typeof(DocumentManagementException))]
        public async Task Add_NewObjectiveWithNewType_ThrowsException()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.ObjectiveTypeID = new ID<ObjectiveTypeDto>();

            // Act
            await service.Add(objectiveToCreate);

            // Assert
            Assert.Fail();
        }

        [TestMethod]
        public async Task Add_NewObjectiveWithExistingParentobjective_AddsChildObjectiveToParentObjective()
        {
            // Arrange
            var existingObjective = mapper.Map<Objective>(ArrangeSimpleObjective());
            await Fixture.Context.Objectives.AddAsync(existingObjective);
            await Fixture.Context.SaveChangesAsync();

            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.ParentObjectiveID = new ID<ObjectiveDto>(existingObjective.ID);

            // Act
            var resultObjective = await service.Add(objectiveToCreate);

            // Assert
            Assert.AreEqual(existingObjective.ID, (int)resultObjective.ParentObjectiveID);
            Assert.IsNotNull(resultObjective.ParentObjectiveID);
        }

        [TestMethod]
        [ExpectedException(typeof(DocumentManagementException))]
        public async Task Add_NewObjectiveWithNewParentobjective_ThrowsException()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.ParentObjectiveID = new ID<ObjectiveDto>();

            // Act
            await service.Add(objectiveToCreate);

            // Assert
            Assert.Fail();
        }

        #endregion

        #region FIND

        [TestMethod]
        public async Task Find_ExistingObjective_ReturnsObjectiveWithIncludedFields()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            objective.DynamicFields = new List<DynamicFieldDto>()
            {
                new DynamicFieldDto()
                {
                    Key = "Key",
                    Name = "Name",
                    Type = DynamicFieldType.STRING,
                    Value = "Value",
                },
            };
            objective.BimElements = new List<BimElementDto>
            {
                new BimElementDto()
                {
                    GlobalID = Guid.NewGuid().ToString(),
                    ParentName = "Parent",
                    ElementName = "Element",
                },
            };
            objective.Items = new List<ItemDto>
            {
                new ItemDto()
                {
                    RelativePath = "/path/file.txt",
                    ItemType = ItemType.File,
                },
            };
            objective.Location = new LocationDto()
            {
                Guid = Guid.NewGuid().ToString(),
                Position = (0, 0, 0),
                CameraPosition = (1, 1, 1),
                Item = new ItemDto()
                {
                    RelativePath = "/path/file.ifc",
                    ItemType = ItemType.Bim,
                },
            };
            var objectiveCreated = await service.Add(objective);
            var existingObjective = Fixture.Context.Objectives.Unsynchronized()
                .First(o => o.ID == (int)objectiveCreated.ID);
            var dtoId = objectiveCreated.ID;

            // Act
            var result = await service.Find(dtoId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(dtoId, result.ID);
            Assert.AreEqual(existingObjective.Title, result.Title);
            Assert.AreEqual(existingObjective.Description, result.Description);
            Assert.AreEqual(existingObjective.CreationDate, result.CreationDate);
            Assert.AreEqual(existingObjective.DueDate, result.DueDate);
            Assert.AreEqual(existingObjective.AuthorID, (int)result.AuthorID);
            Assert.AreEqual(existingObjective.ProjectID, (int)result.ProjectID);
            Assert.AreEqual(existingObjective.ObjectiveTypeID, (int)result.ObjectiveTypeID);
            Assert.AreEqual(existingObjective.DynamicFields.Count, result.DynamicFields.Count());
            Assert.AreEqual(existingObjective.DynamicFields.First().ID, (int)result.DynamicFields.First().ID);
            Assert.AreEqual(existingObjective.BimElements.Count, result.BimElements.Count());
            Assert.AreEqual(existingObjective.BimElements.First().BimElement.GlobalID, result.BimElements.First().GlobalID);
            Assert.AreEqual(existingObjective.Items.Count, result.Items.Count());
            Assert.AreEqual(existingObjective.Items.First().ItemID, (int)result.Items.First().ID);
            Assert.IsNotNull(existingObjective.Location);
            Assert.AreEqual(existingObjective.Location.Guid, result.Location.Guid);
            Assert.AreEqual(existingObjective.Location.ItemID, (int)result.Location.Item.ID);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Objective>))]
        public async Task Find_NotExistingObjective_ThrowsNotFoundException()
        {
            // Arrange
            var invalidId = ID<ObjectiveDto>.InvalidID;

            // Act
            await service.Find(invalidId);

            // Assert
            Assert.Fail();
        }

        #endregion

        #region REMOVE

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Objective>))]
        public async Task Remove_NotExistingObjective_ThrowsNotFoundException()
        {
            // Arrange
            var invalidId = ID<ObjectiveDto>.InvalidID;

            // Act
            await service.Remove(invalidId);

            // Assert
            Assert.Fail();
        }

        [TestMethod]
        public async Task Remove_ExistingObjectiveSimple_ReturnsIdsOfDeletedObjectived()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            var existingObjective = await service.Add(objective);

            // Act
            var actualResult = await service.Remove(existingObjective.ID);

            // Assert
            Assert.AreEqual(actualResult.Count(), 1);
            Assert.AreEqual(actualResult.First(), existingObjective.ID);
        }

        [TestMethod]
        public async Task Remove_ExistingObjectiveSimple_DeletesFromDatabase()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            var existingObjective = await service.Add(objective);

            // Act
            await service.Remove(existingObjective.ID);
            var resultObjective = Fixture.Context.Objectives.Unsynchronized().FirstOrDefault(x => x.ID == (int)existingObjective.ID);

            // Assert
            Assert.IsNull(resultObjective);
        }

        [TestMethod]
        public async Task Remove_ExistingObjectiveWithBimElement_ReturnsIdsOfDeletedObjectived()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            objective.BimElements = new List<BimElementDto>
            {
                new BimElementDto()
                {
                    GlobalID = Guid.NewGuid().ToString(),
                    ParentName = "Parent",
                    ElementName = "Element",
                },
            };
            var existingObjective = await service.Add(objective);

            // Act
            var actualResult = await service.Remove(existingObjective.ID);

            // Assert
            // Assert
            Assert.AreEqual(actualResult.Count(), 1);
            Assert.AreEqual(actualResult.First(), existingObjective.ID);
        }

        [TestMethod]
        public async Task Remove_ExistingObjectiveWithLocation_ReturnsIdsOfDeletedObjectived()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            objective.Location = new LocationDto()
            {
                Guid = Guid.NewGuid().ToString(),
                Position = (0, 0, 0),
                CameraPosition = (1, 1, 1),
                Item = new ItemDto()
                {
                    RelativePath = "/path/file.ifc",
                    ItemType = ItemType.Bim,
                },
            };
            var existingObjective = await service.Add(objective);

            // Act
            var actualResult = await service.Remove(existingObjective.ID);

            // Assert
            Assert.AreEqual(actualResult.Count(), 1);
            Assert.AreEqual(actualResult.First(), existingObjective.ID);
        }

        [TestMethod]
        public async Task Remove_ExistingObjectiveWithParentObjective_ReturnsIdsOfDeletedObjectived()
        {
            // Arrange
            var parentObjective = await service.Add(ArrangeSimpleObjective());

            var objective = ArrangeSimpleObjective();
            objective.ParentObjectiveID = parentObjective.ID;
            var existingObjective = await service.Add(objective);

            // Act
            var actualResult = await service.Remove(existingObjective.ID);

            // Assert
            Assert.AreEqual(actualResult.Count(), 1);
            Assert.AreEqual(actualResult.First(), existingObjective.ID);

        }

        [TestMethod]
        public async Task Remove_ExistingObjectiveWithChildObjective_ReturnsIdsOfDeletedParentAndChildren()
        {
            // Arrange
            var existingObjective = await service.Add(ArrangeSimpleObjective());

            var objective = ArrangeSimpleObjective();
            objective.ParentObjectiveID = existingObjective.ID;
            var childObjective = await service.Add(objective);

            // Act
            var actualResult = await service.Remove(existingObjective.ID);

            // Assert
            Assert.AreEqual(actualResult.Count(), 2);
            Assert.IsTrue(actualResult.Contains(existingObjective.ID));
            Assert.IsTrue(actualResult.Contains(childObjective.ID));
        }

        [TestMethod]
        public async Task Remove_ExistingObjectiveWithChildObjective_DeletesChild()
        {
            // Arrange
            var existingObjective = await service.Add(ArrangeSimpleObjective());

            var objective = ArrangeSimpleObjective();
            objective.ParentObjectiveID = existingObjective.ID;
            var childObjective = await service.Add(objective);

            // Act
            await service.Remove(existingObjective.ID);
            var resultObjective = Fixture.Context.Objectives.Unsynchronized().FirstOrDefault(x => x.ID == (int)childObjective.ID);

            // Assert
            Assert.IsNull(resultObjective);
        }

        [TestMethod]
        public async Task Remove_ExistingObjectiveWithItem_ReturnsIdsOfDeletedObjectived()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            objective.Items = new List<ItemDto>
            {
                new ItemDto()
                {
                    RelativePath = "/path/file.txt",
                    ItemType = ItemType.File,
                },
            };
            var existingObjective = await service.Add(objective);

            // Act
            var actualResult = await service.Remove(existingObjective.ID);

            // Assert
            Assert.AreEqual(actualResult.Count(), 1);
            Assert.AreEqual(actualResult.First(), existingObjective.ID);
        }

        [TestMethod]
        public async Task Remove_ExistingObjectiveWithDynamicField_ReturnsIdsOfDeletedObjectived()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            objective.DynamicFields = new List<DynamicFieldDto>()
            {
                new DynamicFieldDto()
                {
                    Key = "keyDate",
                    Name = "Name",
                    Type = DynamicFieldType.DATE,
                    Value = DateTime.Now,
                },
            };
            var existingObjective = await service.Add(objective);

            // Act
            var actualResult = await service.Remove(existingObjective.ID);

            // Assert
            Assert.AreEqual(actualResult.Count(), 1);
            Assert.AreEqual(actualResult.First(), existingObjective.ID);
        }

        [TestMethod]
        public async Task Remove_ExistingObjectiveWithDynamicFieldSimple_DeletesDynamicField()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            objective.DynamicFields = new List<DynamicFieldDto>()
            {
                new DynamicFieldDto()
                {
                    Key = "keyDate",
                    Name = "Name",
                    Type = DynamicFieldType.DATE,
                    Value = DateTime.Now,
                },
            };
            var existingObjective = await service.Add(objective);
            var objectiveFromDb = Fixture.Context.Objectives.First(x => x.ID == (int)existingObjective.ID);

            // Act
            await service.Remove(existingObjective.ID);
            var resultDynamicField = Fixture.Context.DynamicFields.Unsynchronized().FirstOrDefault(x => x.ObjectiveID == objectiveFromDb.ID);

            // Assert
            Assert.IsNull(resultDynamicField);
        }

        [TestMethod]
        public async Task Remove_ExistingObjectiveWithDynamicFieldObject_DeletesDynamicFieldAndItsChildren()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            objective.DynamicFields = new List<DynamicFieldDto>()
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
            var existingObjective = await service.Add(objective);
            var objectiveFromDb = Fixture.Context.Objectives.First(x => x.ID == (int)existingObjective.ID);

            // Act
            await service.Remove(existingObjective.ID);
            var resultDynamicFields = Fixture.Context.DynamicFields.Unsynchronized().Select(x => x.ObjectiveID == objectiveFromDb.ID || x.ParentFieldID != null);

            // Assert
            Assert.AreEqual(0, resultDynamicFields.Count());
        }

        #endregion

        #region UPDATE

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Objective>))]
        public async Task Update_NotExistingObjective_ThrowsNotFoundException()
        {
            // Arrange
            var notExistingObjective = new ObjectiveDto();

            // Act
            await service.Update(notExistingObjective);

            // Assert
            Assert.Fail();
        }

        [TestMethod]
        public async Task Update_ExistingObjectiveSimpleFields_ReturnsTrue()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            var existingObjective = await service.Add(objective);
            var beforeObjective = await service.Find(existingObjective.ID);
            beforeObjective.Title += "Edit";
            beforeObjective.Description += "Edit";
            beforeObjective.CreationDate = beforeObjective.CreationDate.AddDays(1);
            beforeObjective.UpdatedAt = beforeObjective.UpdatedAt.AddDays(1);
            beforeObjective.DueDate = beforeObjective.DueDate.AddDays(1);
            beforeObjective.Status = ObjectiveStatus.Late;
            beforeObjective.ObjectiveTypeID = new ID<ObjectiveTypeDto>(
                Fixture.Context.ObjectiveTypes.First(x => x.ID != (int)beforeObjective.ObjectiveTypeID).ID);

            // Act
            var result = await service.Update(beforeObjective);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Update_ExistingObjectiveSimpleFields_AllSimpleFieldsAreUpdated()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            var existingObjective = await service.Add(objective);
            var beforeObjective = await service.Find(existingObjective.ID);
            var expectedTitle = beforeObjective.Title + "Edit";
            var expectedDescription = beforeObjective.Description + "Edit";
            var expectedCreationDate = beforeObjective.CreationDate.AddDays(1);
            var expectedDueDate = beforeObjective.DueDate.AddDays(1);
            var expectedLastUpdateDate = DateTime.UtcNow;
            var expectedStatus = ObjectiveStatus.Late;
            var expectedType = Fixture.Context.ObjectiveTypes.First(x => x.ID != (int)beforeObjective.ObjectiveTypeID).ID;

            beforeObjective.Title = expectedTitle;
            beforeObjective.Description = expectedDescription;
            beforeObjective.CreationDate = expectedCreationDate;
            beforeObjective.DueDate = expectedDueDate;
            beforeObjective.Status = expectedStatus;
            beforeObjective.ObjectiveTypeID = new ID<ObjectiveTypeDto>(expectedType);

            // Act
            var result = await service.Update(beforeObjective);
            var afterObjective = await service.Find(existingObjective.ID);

            // Assert
            Assert.AreEqual(expectedTitle, afterObjective.Title);
            Assert.AreEqual(expectedDescription, afterObjective.Description);
            Assert.AreEqual(expectedCreationDate, afterObjective.CreationDate);
            Assert.AreEqual(expectedDueDate, afterObjective.DueDate);
            Assert.AreEqual(expectedLastUpdateDate.Date, afterObjective.UpdatedAt.Date);
            Assert.AreEqual(expectedStatus, afterObjective.Status);
            Assert.AreEqual(expectedType, (int)afterObjective.ObjectiveTypeID);
        }

        [TestMethod]
        public async Task Update_ExistingObjectiveAddExistingParent_ReturnsTrue()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            var parentobjective = await service.Add(objective);
            var newObjective = await service.Add(objective);
            var existingObjective = await service.Find(newObjective.ID);

            existingObjective.ParentObjectiveID = parentobjective.ID;

            // Act
            var result = await service.Update(existingObjective);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Update_ExistingObjectiveAddExistingParent_ObjectiveHasParent()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            var parentobjective = await service.Add(objective);
            var newObjective = await service.Add(objective);
            var existingObjective = await service.Find(newObjective.ID);

            var excpectedParentId = existingObjective.ParentObjectiveID = parentobjective.ID;

            // Act
            await service.Update(existingObjective);
            var actualObjective = await service.Find(existingObjective.ID);

            // Assert
            Assert.IsNotNull(actualObjective.ParentObjectiveID);
            Assert.AreEqual(excpectedParentId, actualObjective.ParentObjectiveID);
        }

        [TestMethod]
        [ExpectedException(typeof(DocumentManagementException))]
        public async Task Update_ExistingObjectiveAddNotExistinhParent_ThrowsException()
        {
            // Arrange
            var objective = ArrangeSimpleObjective();
            var newObjective = await service.Add(objective);
            var existingObjective = await service.Find(newObjective.ID);

            existingObjective.ParentObjectiveID = ID<ObjectiveDto>.InvalidID;

            // Act
            await service.Update(existingObjective);

            // Assert
            Assert.Fail();
        }

        #endregion

        #region GET

        [TestMethod]
        public async Task GetObjectives_ExistingProject_ReturnListOfObjectives()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            await service.Add(objectiveToCreate);
            var existingProject = Fixture.Context.Projects.Unsynchronized().First();
            var existingProjectId = new ID<ProjectDto>(existingProject.ID);

            var expectedCount = 1;

            // Act
            var result = await service.GetObjectives(existingProjectId, new ObjectiveFilterParameters(), new SortParameters());
            var actualCount = result.Items.Count();

            // Assert
            Assert.IsTrue(result.Items.Any());
            Assert.AreEqual(expectedCount, actualCount);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Project>))]
        public async Task GetObjectives_NotExistingProject_ThrowsException()
        {
            // Arrange
            var nonExistingProjectId = ID<ProjectDto>.InvalidID;

            // Act
            await service.GetObjectives(nonExistingProjectId, new ObjectiveFilterParameters(), new SortParameters());

            // Assert
            Assert.Fail();
        }

        [TestMethod]
        public async Task GetObjectives_FilterByExistingBimElement_ListOfObjectivesLinkedToBimElement()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.BimElements = new List<BimElementDto>()
            {
                new BimElementDto()
                {
                   GlobalID = $"{Guid.NewGuid()}",
                   ParentName = "parentName",
                   ElementName = "elementName",
                },
            };
            await service.Add(objectiveToCreate);

            var existingProject = Fixture.Context.Projects.Unsynchronized().First();
            var existingProjectId = new ID<ProjectDto>(existingProject.ID);

            var expectedCount = 1;

            var filter = new ObjectiveFilterParameters()
            {
                BimElementGuid = objectiveToCreate.BimElements.First().GlobalID,
            };

            // Act
            var result = await service.GetObjectives(existingProjectId, filter, new SortParameters());
            var actualCount = result.Items.Count();

            // Assert
            Assert.IsTrue(result.Items.Any());
            Assert.AreEqual(expectedCount, actualCount);
        }

        [TestMethod]
        public async Task GetObjectives_FilterByNotExistingBimelement_EmptyList()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            await service.Add(objectiveToCreate);

            var existingProject = Fixture.Context.Projects.Unsynchronized().First();
            var existingProjectId = new ID<ProjectDto>(existingProject.ID);

            var expectedCount = 0;

            var filter = new ObjectiveFilterParameters()
            {
                BimElementGuid = $"{Guid.NewGuid()}",
            };

            // Act
            var result = await service.GetObjectives(existingProjectId, filter, new SortParameters());
            var actualCount = result.Items.Count();

            // Assert
            Assert.IsFalse(result.Items.Any());
            Assert.AreEqual(expectedCount, actualCount);
        }

        [TestMethod]
        public async Task GetObjectives_FilterByExistingTitlePart_ListOfObjectives()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            var expectedTitleObjective = await service.Add(objectiveToCreate);
            var filter = new ObjectiveFilterParameters()
            {
                TitlePart = expectedTitleObjective.Title.ToLower(),
            };

            objectiveToCreate.Title = $"{Guid.NewGuid()}";
            await service.Add(objectiveToCreate);
            objectiveToCreate.Title = $"{Guid.NewGuid()}";
            await service.Add(objectiveToCreate);

            var existingProject = Fixture.Context.Projects.Unsynchronized().First();
            var existingProjectId = new ID<ProjectDto>(existingProject.ID);

            var expectedCount = 1;

            // Act
            var result = await service.GetObjectives(existingProjectId, filter, new SortParameters());
            var actualCount = result.Items.Count();

            // Assert
            Assert.IsTrue(result.Items.Any());
            Assert.AreEqual(expectedCount, actualCount);
        }

        [TestMethod]
        public async Task GetObjectives_FilterByNotExistingTitlePart_EmptyList()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            await service.Add(objectiveToCreate);

            var existingProject = Fixture.Context.Projects.Unsynchronized().First();
            var existingProjectId = new ID<ProjectDto>(existingProject.ID);

            var expectedCount = 0;

            var filter = new ObjectiveFilterParameters()
            {
                TitlePart = $"{Guid.NewGuid()}",
            };

            // Act
            var result = await service.GetObjectives(existingProjectId, filter, new SortParameters());
            var actualCount = result.Items.Count();

            // Assert
            Assert.IsFalse(result.Items.Any());
            Assert.AreEqual(expectedCount, actualCount);
        }

        [TestMethod]
        public async Task GetObjectives_FilterByStatus_ListOfObjectivesWithThatStatus()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            var expectedStatusObjective = await service.Add(objectiveToCreate);

            var filter = new ObjectiveFilterParameters()
            {
                Statuses = new List<int>() { (int)expectedStatusObjective.Status },
            };

            objectiveToCreate.Status++;
            await service.Add(objectiveToCreate);
            objectiveToCreate.Status++;
            await service.Add(objectiveToCreate);

            var existingProject = Fixture.Context.Projects.Unsynchronized().First();
            var existingProjectId = new ID<ProjectDto>(existingProject.ID);

            var expectedCount = 1;

            // Act
            var result = await service.GetObjectives(existingProjectId, filter, new SortParameters());
            var actualCount = result.Items.Count();

            // Assert
            Assert.IsTrue(result.Items.Any());
            Assert.AreEqual(expectedCount, actualCount);
        }

        [TestMethod]
        public async Task GetObjectives_FilterByStatuses_ListOfObjectivesWithThatStatuses()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            var expectedStatusObjective = await service.Add(objectiveToCreate);

            var filter = new ObjectiveFilterParameters()
            {
                Statuses = new List<int>() { (int)expectedStatusObjective.Status, (int)expectedStatusObjective.Status + 1 },
            };

            objectiveToCreate.Status++;
            await service.Add(objectiveToCreate);
            objectiveToCreate.Status++;
            await service.Add(objectiveToCreate);

            var existingProject = Fixture.Context.Projects.Unsynchronized().First();
            var existingProjectId = new ID<ProjectDto>(existingProject.ID);

            var expectedCount = 2;

            // Act
            var result = await service.GetObjectives(existingProjectId, filter, new SortParameters());
            var actualCount = result.Items.Count();

            // Assert
            Assert.IsTrue(result.Items.Any());
            Assert.AreEqual(expectedCount, actualCount);

            for (int i = 0; i < result.Items.Count(); i++)
            {
                var resultType = (int)result.Items.ElementAt(i).Status;
                var filterType = filter.Statuses.ElementAt(i);
                Assert.AreEqual(resultType, filterType);
            }
        }

        [TestMethod]
        public async Task GetObjectives_FilterByExistingType_ListOfObjectivesWithType()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            var expectedStatusObjective = await service.Add(objectiveToCreate);

            var filter = new ObjectiveFilterParameters()
            {
                TypeIds = new List<int>() { (int)expectedStatusObjective.ObjectiveType.ID },
            };

            var existingProject = Fixture.Context.Projects.Unsynchronized().First();
            var existingProjectId = new ID<ProjectDto>(existingProject.ID);

            var expectedCount = 1;

            // Act
            var result = await service.GetObjectives(existingProjectId, filter, new SortParameters());
            var actualCount = result.Items.Count();

            // Assert
            Assert.IsTrue(result.Items.Any());
            Assert.AreEqual(expectedCount, actualCount);
        }

        [TestMethod]
        public async Task GetObjectives_FilterByExistingTypes_ListOfObjectivesWithTypes()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            var expectedStatusObjective = await service.Add(objectiveToCreate);

            var filter = new ObjectiveFilterParameters()
            {
                TypeIds = new List<int>() { (int)expectedStatusObjective.ObjectiveType.ID, (int)expectedStatusObjective.ObjectiveType.ID + 1 },
            };

            objectiveToCreate.ObjectiveTypeID = new ID<ObjectiveTypeDto>(((int)objectiveToCreate.ObjectiveTypeID) + 1);
            await service.Add(objectiveToCreate);

            var existingProject = Fixture.Context.Projects.Unsynchronized().First();
            var existingProjectId = new ID<ProjectDto>(existingProject.ID);

            var expectedCount = 2;

            // Act
            var result = await service.GetObjectives(existingProjectId, filter, new SortParameters());
            var actualCount = result.Items.Count();

            // Assert
            Assert.IsTrue(result.Items.Any());
            Assert.AreEqual(expectedCount, actualCount);

            for (int i = 0; i < result.Items.Count(); i++)
            {
                var resultType = (int)result.Items.ElementAt(i).ObjectiveType.ID;
                var filterType = filter.TypeIds.ElementAt(i);
                Assert.AreEqual(resultType, filterType);
            }
        }

        [TestMethod]
        public async Task GetObjectivesWithLocation_FilterByLocation_ListOfObjectivesWithLocations()
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            var itemName = "itemName";
            objectiveToCreate.Location = new LocationDto()
            {
                Guid = Guid.NewGuid().ToString(),
                Item = new ItemDto()
                {
                    ItemType = ItemType.Bim,
                    RelativePath = $"/{itemName}.ifc",
                },
            };
            await service.Add(objectiveToCreate);

            var existingProject = Fixture.Context.Projects.Unsynchronized().First();
            var existingProjectId = new ID<ProjectDto>(existingProject.ID);

            var expectedCount = 1;

            // Act
            var result = await service.GetObjectivesWithLocation(existingProjectId, itemName, new ObjectiveFilterParameters());
            var actualCount = result.Count();

            // Assert
            Assert.IsTrue(result.Any());
            Assert.AreEqual(expectedCount, actualCount);
        }

        [TestMethod]
        [DataRow(1, 3, 6, 3, DisplayName = "Page is smaller than objective count")]
        [DataRow(1, 10, 6, 6, DisplayName = "Page is bigger than objective count")]
        [DataRow(2, 3, 5, 2, DisplayName = "Last page is not full")]
        [DataRow(0, 0, 5, 5, DisplayName = "No pagination data (use default values)")]
        [DataRow(-1, -1, 5, 5, DisplayName = "Wrong pagination data (use default values)")]
        public async Task GetObjectives_OnePageOfObjectives_ListOfExpectedObjectives(int pageNumber, int pageSize, int objectivesCount, int expectedCount)
        {
            // Arrange
            var objectiveToCreate = ArrangeSimpleObjective();
            for (int i = 0; i < objectivesCount; i++)
                await service.Add(objectiveToCreate);

            var existingProject = Fixture.Context.Projects.Unsynchronized().First();
            var existingProjectId = new ID<ProjectDto>(existingProject.ID);
            var filter = new ObjectiveFilterParameters()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
            };

            // Act
            var result = await service.GetObjectives(existingProjectId, filter, new SortParameters());
            var actualCount = result.Items.Count();

            // Assert
            Assert.IsTrue(result.Items.Any());
            Assert.AreEqual(expectedCount, actualCount);
        }

        #endregion

        #region PRIVATE METHODS

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

        #endregion
    }
}
