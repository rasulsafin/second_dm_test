using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Common;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Tests.Utility
{
    [TestClass]
    public class ItemsHelperTests
    {
        private static IMapper mapper;
        private static ItemsHelper helper;
        private static ItemComparer comparer;
        private ServiceProvider serviceProvider;

        private static SharedDatabaseFixture Fixture { get; set; }

        [TestInitialize]
        public void Setup()
        {
            comparer = new ItemComparer();
            var mapperConfig = new MapperConfiguration(mc => mc.AddProfile(new MappingProfile()));
            mapper = mapperConfig.CreateMapper();

            Fixture = new SharedDatabaseFixture(context =>
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var users = MockData.DEFAULT_USERS;
                var projects = MockData.DEFAULT_PROJECTS;
                var objectives = MockData.DEFAULT_OBJECTIVES;
                var items = MockData.DEFAULT_ITEMS;
                var objectiveTypes = MockData.DEFAULT_OBJECTIVE_TYPES;

                projects[0].Items = items;

                context.Users.AddRange(users);
                context.Projects.AddRange(projects);
                context.ObjectiveTypes.AddRange(objectiveTypes);
                context.SaveChanges();

                objectives.ForEach(o =>
                {
                    o.ProjectID = projects[0].ID;
                    o.ObjectiveTypeID = objectiveTypes[0].ID;
                });
                context.Objectives.AddRange(objectives);
                context.SaveChanges();

                context.ObjectiveItems.Add(new ObjectiveItem { ItemID = items[0].ID, ObjectiveID = objectives[0].ID });
                context.SaveChanges();
            });

            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped<ItemsHelper>();
            services.AddMappingResolvers();
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddSingleton(x => Fixture.Context);

            serviceProvider = services.BuildServiceProvider();
            mapper = serviceProvider.GetService<IMapper>();
            helper = serviceProvider.GetService<ItemsHelper>();
        }

        [TestCleanup]
        public void Cleanup()
            => Fixture.Dispose();

        [TestMethod]
        public async Task CheckItemToLink_ExistingItemNotLinkedToObjectiveParent_ReturnsItem()
        {
            var context = Fixture.Context;
            var existingItem = context.Items.Unsynchronized().First();
            var parent = new ObjectiveItemContainer(context, context.Objectives.First());
            var item = new ItemDto { ID = new ID<ItemDto>(existingItem.ID) };

            var result = await helper.CheckItemToLink(item, parent);

            Assert.IsNotNull(result);
            Assert.IsTrue(comparer.NotNullEquals(existingItem, result));
        }

        [TestMethod]
        public async Task CheckItemToLink_ExistingItemLinkedToObjectiveAndNotLinkedToProjectParent_ReturnsItem()
        {
            var context = Fixture.Context;
            var project = context.Projects.First();
            var parent = new ProjectItemContainer(project);
            var existingItem = context.Items.First(x => x.ProjectID == project.ID);
            existingItem.ProjectID = null;
            context.Update(existingItem);
            await context.SaveChangesAsync();
            var item = new ItemDto { ID = new ID<ItemDto>(existingItem.ID) };

            var result = await helper.CheckItemToLink(item, parent);

            Assert.IsNotNull(result);
            Assert.IsTrue(comparer.NotNullEquals(existingItem, result));
        }

        [TestMethod]
        public async Task CheckItemToLink_ExistingItemLinkedToObjectiveParent_ReturnsNull()
        {
            var context = Fixture.Context;
            var existingItem = context.Items.Unsynchronized().First(i => context.ObjectiveItems.Any(oi => oi.ItemID == i.ID));
            var parent = new ObjectiveItemContainer(context, existingItem.Objectives.First().Objective);
            var item = new ItemDto { ID = new ID<ItemDto>(existingItem.ID) };

            var result = await helper.CheckItemToLink(item, parent);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CheckItemToLink_ExistingItemLinkedToProjectParent_ReturnsNull()
        {
            var context = Fixture.Context;
            var existingItem = context.Items.Unsynchronized().First(i => i.ProjectID != null);
            var parent = new ProjectItemContainer(existingItem.Project);
            var item = new ItemDto { ID = new ID<ItemDto>(existingItem.ID) };

            var result = await helper.CheckItemToLink(item, parent);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CheckItemToLink_NotExistingItemWithObjectiveParent_ReturnsItemAddedToDb()
        {
            var context = Fixture.Context;
            var guid = Guid.NewGuid();
            var name = $"Name{guid}";
            var itemType = ItemType.Bim;
            var parent = new ObjectiveItemContainer(context, context.Objectives.First());
            var itemsCount = context.Items.Count();
            var item = new ItemDto { ItemType = itemType, RelativePath = name };

            var result = await helper.CheckItemToLink(item, parent);

            var addedItem = context.Items
               .Unsynchronized()
               .FirstOrDefault(i => i.ItemType == (int)itemType && i.RelativePath == name);

            Assert.IsNotNull(result);
            Assert.IsNotNull(addedItem);
            Assert.AreEqual(itemsCount + 1, context.Items.Unsynchronized().Count());
        }

        [TestMethod]
        public async Task CheckItemToLink_NotExistingItemWithProjectParent_ReturnsItemAddedToDb()
        {
            var context = Fixture.Context;
            var guid = Guid.NewGuid();
            var name = $"Name{guid}";
            var itemType = ItemType.Bim;
            var parentId = new ProjectItemContainer(context.Projects.First());
            var itemsCount = context.Items.Count();
            var item = new ItemDto { ItemType = itemType, RelativePath = name };

            var result = await helper.CheckItemToLink(item, parentId);

            var addedItem = context.Items
               .Unsynchronized()
               .FirstOrDefault(i => i.ItemType == (int)itemType && i.RelativePath == name);

            Assert.IsNotNull(result);
            Assert.IsNotNull(addedItem);
            Assert.AreEqual(itemsCount + 1, context.Items.Unsynchronized().Count());
        }

        [TestMethod]
        public async Task AddItemsAsync_ToExistingObjectiveWithNewItems_AddsItemsToObjective()
        {
            // Arrange
            var expectedCount = Fixture.Context.Items.Count() + 1;
            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.Items = new List<ItemDto>
            {
                new ItemDto()
                {
                    RelativePath = "/path/file.txt",
                    ItemType = ItemType.File,
                },
            };
            var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);
            await Fixture.Context.Objectives.AddAsync(objectiveToSave);
            await Fixture.Context.SaveChangesAsync();

            // Act
            var resultObjective = await helper.AddItemsAsync(objectiveToCreate.Items, objectiveToSave);
            var createdLink = resultObjective?.Items?.FirstOrDefault();
            var item = await Fixture.Context.Items.FindAsync(createdLink?.ItemID);
            var actualCount = Fixture.Context.Items.Count();

            // Assert
            Assert.IsNotNull(item);
            Assert.AreEqual(expectedCount, actualCount);
            Assert.AreEqual(createdLink.ObjectiveID, resultObjective.ID);
            Assert.AreEqual(createdLink.ItemID, item.ID);
        }

        [TestMethod]
        public async Task AddItemsAsync_ToExistingObjectiveWithExistingItems_AddsItemsToObjective()
        {
            // Arrange
            var existingItem = Fixture.Context.Items.Unsynchronized().First();

            var objectiveToCreate = ArrangeSimpleObjective();
            objectiveToCreate.Items = new List<ItemDto>
            {
                new ItemDto()
                {
                    RelativePath = existingItem.RelativePath,
                    ItemType = (ItemType)existingItem.ItemType,
                },
            };
            var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);

            await Fixture.Context.Objectives.AddAsync(objectiveToSave);
            await Fixture.Context.SaveChangesAsync();

            var expectedCount = Fixture.Context.Items.Count();

            // Act
            var resultObjective = await helper.AddItemsAsync(objectiveToCreate.Items, objectiveToSave);
            var createdLink = resultObjective?.Items?.FirstOrDefault();
            var actualCount = Fixture.Context.Items.Count();

            // Assert
            Assert.AreEqual(expectedCount, actualCount);
            Assert.AreEqual(createdLink.ObjectiveID, resultObjective.ID);
            Assert.AreEqual(createdLink.ItemID, existingItem.ID);
        }

        [TestMethod]
        public async Task UpdateItems_ExistingObjectiveAddItem_ReturnsObjectiveWithItems()
        {
            // Arrange
            var existingItem = Fixture.Context.Items.Unsynchronized().First();

            var objectiveToCreate = ArrangeSimpleObjective();
            var expectedItems = new List<ItemDto>
            {
                new ItemDto()
                {
                    RelativePath = existingItem.RelativePath,
                    ItemType = (ItemType)existingItem.ItemType,
                },
            };
            var objectiveToSave = mapper.Map<Objective>(objectiveToCreate);

            await Fixture.Context.Objectives.AddAsync(objectiveToSave);
            await Fixture.Context.SaveChangesAsync();

            var expectedCount = expectedItems.Count();

            // Act
            var resultObjective = await helper.UpdateItemsAsync(expectedItems, objectiveToSave);
            var actualCount = resultObjective.Items.Count();

            // Assert
            Assert.AreEqual(expectedCount, actualCount);
        }

        [TestMethod]
        public async Task UpdateItems_ExistingObjectiveRemoveItem_ReturnsTrue()
        {
            // Arrange
            var existingObjectiveWithItems = Fixture.Context.Objectives.Unsynchronized().First(x => x.Items.Count > 0);

            // Act
            var resultObjective = await helper.UpdateItemsAsync(null, existingObjectiveWithItems);
            var actualCount = resultObjective.Items?.Count();

            // Assert
            Assert.AreEqual(0, actualCount);
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
