using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Common;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Tests.Utility
{
    [TestClass]
    public class BimElementsHelperTests
    {
        private static BimElementsHelper helper;
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
                var bimElements = MockData.DEFAULT_BIM_ELEMENTS;

                projects.First().Items = MockData.DEFAULT_ITEMS;
                context.Users.AddRange(users);
                context.Projects.AddRange(projects);
                context.ObjectiveTypes.AddRange(objectiveTypes);
                context.BimElements.AddRange(bimElements);
                context.SaveChanges();
            });

            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped<BimElementsHelper>();
            services.AddMappingResolvers();
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddSingleton(x => Fixture.Context);

            serviceProvider = services.BuildServiceProvider();
            mapper = serviceProvider.GetService<IMapper>();
            helper = serviceProvider.GetService<BimElementsHelper>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Fixture.Dispose();
            serviceProvider.Dispose();
        }

        [TestMethod]
        public async Task AddBimElementsAsync_ToExistingObjectiveWithNewBimElements_AddsBimElementsToObjective()
        {
            // Arrange
            var expectedCount = Fixture.Context.BimElements.Count() + 1;
            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.BimElements = new List<BimElementDto>
            {
                new BimElementDto()
                {
                    GlobalID = Guid.NewGuid().ToString(),
                    ParentName = "Parent",
                    ElementName = "Element",
                },
            };
            var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);
            await Fixture.Context.Objectives.AddAsync(objectiveToSave);
            await Fixture.Context.SaveChangesAsync();

            // Act
            var resultObjective = await helper.AddBimElementsAsync(objectiveToCreate.BimElements, objectiveToSave);
            var createdLink = resultObjective?.BimElements?.FirstOrDefault();
            var bimElement = await Fixture.Context.BimElements.FindAsync(createdLink?.BimElementID);
            var actualCount = Fixture.Context.BimElements.Count();

            // Assert
            Assert.IsNotNull(bimElement);
            Assert.AreEqual(expectedCount, actualCount);
            Assert.AreEqual(createdLink.ObjectiveID, resultObjective.ID);
            Assert.AreEqual(createdLink.BimElementID, bimElement.ID);
        }

        [TestMethod]
        public async Task AddBimElementsAsync_ToExistingObjectiveWithExistingBimElements_AddsBimElementsToObjective()
        {
            // Arrange
            var existingBimElement = Fixture.Context.BimElements.First();

            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.BimElements = new List<BimElementDto>
            {
                new BimElementDto()
                {
                    GlobalID = existingBimElement.GlobalID,
                    ParentName = existingBimElement.ParentName,
                    ElementName = existingBimElement.ElementName,
                },
            };
            var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);

            await Fixture.Context.Objectives.AddAsync(objectiveToSave);
            await Fixture.Context.SaveChangesAsync();

            var expectedCount = Fixture.Context.BimElements.Count();

            // Act
            var resultObjective = await helper.AddBimElementsAsync(objectiveToCreate.BimElements, objectiveToSave);
            var createdLink = resultObjective?.BimElements?.FirstOrDefault();
            var actualCount = Fixture.Context.BimElements.Count();

            // Assert
            Assert.AreEqual(expectedCount, actualCount);
            Assert.AreEqual(createdLink.ObjectiveID, resultObjective.ID);
            Assert.AreEqual(createdLink.BimElementID, existingBimElement.ID);
        }

        [TestMethod]
        public async Task UpdateBimElements_ExistingObjectiveAddBimElement_ReturnsObjectiveWithBimElements()
        {
            // Arrange
            var existingBimElement = Fixture.Context.BimElements.First();

            var objectiveToCreate = ArrangeSimpleObjective();
            var expectedBimElement = new BimElementDto()
            {
                GlobalID = existingBimElement.GlobalID,
                ParentName = existingBimElement.ParentName,
                ElementName = existingBimElement.ElementName,
            };
            var newBimElements = new List<BimElementDto>() { expectedBimElement };
            var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);

            await Fixture.Context.Objectives.AddAsync(objectiveToSave);
            await Fixture.Context.SaveChangesAsync();

            var expectedCount = newBimElements.Count();

            // Act
            await helper.UpdateBimElementsAsync(newBimElements, objectiveToSave.ID);
            var resultObjective = Fixture.Context.Objectives.Unsynchronized().First(o => o.ID == objectiveToSave.ID);
            var actualCount = resultObjective.BimElements.Count();
            var actualBimElement = resultObjective.BimElements?.FirstOrDefault()?.BimElement;

            // Assert
            Assert.AreEqual(expectedCount, actualCount);
            Assert.AreEqual(expectedBimElement.GlobalID, actualBimElement.GlobalID);
            Assert.AreEqual(expectedBimElement.ParentName, existingBimElement.ParentName);
            Assert.AreEqual(expectedBimElement.ElementName, existingBimElement.ElementName);
        }

        [TestMethod]
        public async Task UpdateBimElements_ExistingObjectiveRemoveBimElement_ReturnsTrue()
        {
            // Arrange
            var existingBimElement = Fixture.Context.BimElements.First();

            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.BimElements = new List<BimElementDto>
            {
                new BimElementDto()
                {
                    GlobalID = existingBimElement.GlobalID,
                    ParentName = existingBimElement.ParentName,
                    ElementName = existingBimElement.ElementName,
                },
            };
            var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);

            await Fixture.Context.Objectives.AddAsync(objectiveToSave);
            await Fixture.Context.SaveChangesAsync();

            // Act
            await helper.UpdateBimElementsAsync(new List<BimElementDto>(), objectiveToSave.ID);
            var resultObjective = Fixture.Context.Objectives.Unsynchronized().First(o => o.ID == objectiveToSave.ID);
            var createdLink = resultObjective?.BimElements?.FirstOrDefault();
            var actualCount = resultObjective.BimElements?.Count();

            // Assert
            Assert.IsNull(actualCount);
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
    }
}
