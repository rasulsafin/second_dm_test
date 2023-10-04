using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Services;
using Brio.Docs.Tests.Utility;
using Brio.Docs.Utility.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Brio.Docs.Tests.Services
{
    [TestClass]
    public class ItemServiceTests
    {
        private static ItemService service;
        private static IMapper mapper;

        private static DMContext Context => Fixture.Context;

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

                var projects = MockData.DEFAULT_PROJECTS;
                var objectives = MockData.DEFAULT_OBJECTIVES;
                var items = MockData.DEFAULT_ITEMS;
                var objectiveTypes = MockData.DEFAULT_OBJECTIVE_TYPES;
                context.Projects.AddRange(projects);
                context.ObjectiveTypes.AddRange(objectiveTypes);
                context.SaveChanges();

                objectives.ForEach(o =>
                {
                    o.ProjectID = projects[0].ID;
                    o.ObjectiveTypeID = objectiveTypes[0].ID;
                });
                context.Objectives.AddRange(objectives);

                items[0].ProjectID = projects[0].ID;
                items[1].ProjectID = projects[0].ID;

                context.Items.AddRange(items);
                context.SaveChanges();

                context.ObjectiveItems.AddRange(new List<ObjectiveItem>
                {
                    new ObjectiveItem { ItemID = items[0].ID, ObjectiveID = objectives[0].ID },
                    new ObjectiveItem { ItemID = items[1].ID, ObjectiveID = objectives[0].ID },
                });

                context.SaveChanges();
            });

            service = new ItemService(
                Fixture.Context,
                mapper,
                Mock.Of<IFactory<IServiceScope, Type, IConnection>>(),
                Mock.Of<IFactory<IServiceScope, DMContext>>(),
                Mock.Of<IFactory<IServiceScope, IMapper>>(),
                Mock.Of<IRequestService>(),
                Mock.Of<IServiceScopeFactory>(),
                Mock.Of<ILogger<ItemService>>());
        }

        [TestCleanup]
        public void Cleanup()
            => Fixture.Dispose();

        [TestMethod]
        public async Task Find_ExistingItem_ReturnsItem()
        {
            var existingItem = Context.Items.Unsynchronized().First();
            var dtoId = new ID<ItemDto>(existingItem.ID);

            var result = await service.Find(dtoId);

            Assert.AreEqual(dtoId, result.ID);
            Assert.AreEqual(existingItem.RelativePath, result.RelativePath);
            Assert.AreEqual(existingItem.ItemType, (int)result.ItemType);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Item>))]
        public async Task Find_NotExistingItem_RaisesNotFoundException()
        {
            var dtoId = ID<ItemDto>.InvalidID;

            await service.Find(dtoId);

            Assert.Fail();
        }

        [TestMethod]
        public async Task GetProjectItems_ExistingProjectWithItems_ReturnsEnumerableWithItems()
        {
            var existingProject = Context.Projects.Include(x => x.Items).First(p => p.Items.Any());
            var projectItems = existingProject.Items;

            var result = await service.GetItems(new ID<ProjectDto>(existingProject.ID));

            var items = result.ToList();
            Assert.AreEqual(projectItems.Count, items.Count);
            projectItems.ToList().ForEach(i =>
            {
                Assert.IsTrue(items.Any(ri => (int)ri.ID == i.ID
                                               && (int)ri.ItemType == i.ItemType
                                               && ri.RelativePath == i.RelativePath));
            });
        }

        [TestMethod]
        public async Task GetProjectItems_ExistingProjectWithoutItems_ReturnsEmptyEnumerable()
        {
            var existingProject = Context.Projects.Unsynchronized().First(p => !p.Items.Any());

            var result = await service.GetItems(new ID<ProjectDto>(existingProject.ID));

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Project>))]
        public async Task GetProjectItems_NotExistingProject_RaisesNotFoundException()
        {
            await service.GetItems(ID<ProjectDto>.InvalidID);

            Assert.Fail();
        }

        [TestMethod]
        public async Task GetObjectiveItems_ExistingObjectiveWithItems_ReturnsEnumerableWithItems()
        {
            var existingObjective = Context.Objectives.Unsynchronized().First(o => o.Items.Any());
            var objectiveItems = existingObjective.Items.Select(oi => oi.Item);

            var result = await service.GetItems(new ID<ObjectiveDto>(existingObjective.ID));

            var expected = objectiveItems.ToList();
            var actual = result.ToList();
            Assert.AreEqual(expected.Count, actual.Count);
            expected.ToList().ForEach(i =>
            {
                Assert.IsTrue(actual.Any(ri => (int)ri.ID == i.ID
                                               && (int)ri.ItemType == i.ItemType
                                               && ri.RelativePath == i.RelativePath));
            });
        }

        [TestMethod]
        public async Task GetObjectiveItems_ExistingObjectiveWithoutItems_ReturnsEmptyEnumerable()
        {
            var existingObjective = Context.Objectives.Unsynchronized().First(o => !o.Items.Any());

            var result = await service.GetItems(new ID<ObjectiveDto>(existingObjective.ID));

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Objective>))]
        public async Task GetObjectiveItems_NotExistingObjective_RaisesNotFoundException()
        {
            await service.GetItems(ID<ObjectiveDto>.InvalidID);

            Assert.Fail();
        }

        [TestMethod]
        public async Task Update_ExistingItem_ReturnsTrue()
        {
            var existingItem = Context.Items.Unsynchronized().First();
            var guid = Guid.NewGuid();
            var newItemType = existingItem.ItemType != 1 ? 1 : 2;
            var newName = $"newName{guid}";
            var item = new ItemDto
            {
                ID = new ID<ItemDto>(existingItem.ID),
                ItemType = (ItemType)newItemType,
                RelativePath = newName,
            };

            var result = await service.Update(item);

            var updatedItem = Context.Items.Unsynchronized().First(i => i.ID == existingItem.ID);
            Assert.IsTrue(result);
            Assert.IsTrue(updatedItem.ItemType == newItemType);
            Assert.IsTrue(updatedItem.RelativePath == newName);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Item>))]
        public async Task Update_NotExistingItem_RaisesNotFoundException()
        {
            var item = new ItemDto { ID = ID<ItemDto>.InvalidID };

            await service.Update(item);

            Assert.Fail();
        }
    }
}
