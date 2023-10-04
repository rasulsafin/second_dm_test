using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Common;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Synchronization;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Tests.Synchronization.Helpers;
using Brio.Docs.Tests.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Range = Moq.Range;

namespace Brio.Docs.Tests.Synchronization
{
    [TestClass]
    public class SynchronizerObjectiveTests
    {
        private Synchronizer synchronizer;
        private IMapper mapper;
        private ServiceProvider serviceProvider;
        private AssertHelper assertHelper;

        private Mock<ISynchronizer<ObjectiveExternalDto>> ObjectiveSynchronizer { get; set; }

        private SharedDatabaseFixture Fixture { get; set; }

        private Mock<IConnection> Connection { get; set; }

        private Mock<IConnectionContext> Context { get; set; }

        private List<ObjectiveExternalDto> ResultObjectiveExternalDtos { get; set; }

        private ObjectiveExternalDto ResultObjectiveExternalDto => ResultObjectiveExternalDtos.First();

        private (Project local, Project synchronized, ProjectExternalDto remote) Project { get; set; }

        [TestInitialize]
        public async Task Setup()
        {
            Fixture = SynchronizerTestsHelper.CreateFixture();
            serviceProvider = SynchronizerTestsHelper.CreateServiceProvider(Fixture.Context);
            synchronizer = serviceProvider.GetService<Synchronizer>();
            mapper = serviceProvider.GetService<IMapper>();

            assertHelper = new AssertHelper(Fixture.Context);

            ResultObjectiveExternalDtos = new List<ObjectiveExternalDto>();
            Connection = new Mock<IConnection>();
            var projectSynchronizer = SynchronizerTestsHelper.CreateSynchronizerStub<ProjectExternalDto>();
            ObjectiveSynchronizer = SynchronizerTestsHelper.CreateSynchronizerStub<ObjectiveExternalDto>(x => ResultObjectiveExternalDtos.Add(x));
            Context = SynchronizerTestsHelper.CreateConnectionContextStub(projectSynchronizer.Object, ObjectiveSynchronizer.Object);
            Connection.Setup(x => x.GetContext(It.IsAny<ConnectionInfoExternalDto>())).ReturnsAsync(Context.Object);
            Project = await SynchronizerTestsHelper.ArrangeProject(projectSynchronizer, Fixture);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Fixture.Dispose();
            serviceProvider.Dispose();
        }

        [TestMethod]
        public async Task Synchronize_ObjectivesUnchanged_DoNothing()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            CheckObjectives(local, objectiveLocal);
            CheckObjectives(synchronized, objectiveSynchronized);
            CheckObjectives(synchronized, Map(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_FilterRejectsLocalObjective_SynchronizeRejectedToo()
        {
            // Arrange.
            var (objectiveLocal, _, _) = await ArrangeObjective();
            var ignore = "Ignore";
            objectiveLocal.Title = ignore;
            Fixture.Context.Objectives.Update(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            var data = new SynchronizingData
            {
                UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync(),
                ObjectivesFilter = x => x.Title != ignore,
            };

            // Act.
            var synchronizationResult = await synchronizer.Synchronize(
                data,
                Connection.Object,
                new ConnectionInfoExternalDto(),
                new Progress<double>(),
                new CancellationTokenSource().Token);

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
        }

        [TestMethod]
        public async Task Synchronize_FilterRejectsSynchronizedObjective_SynchronizeRejectedToo()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            var ignore = "Ignore";
            objectiveLocal.Title = "New value";
            objectiveSynchronized.Title = ignore;
            objectiveRemote.UpdatedAt = DateTime.UtcNow.AddDays(-1);
            Fixture.Context.Objectives.UpdateRange(objectiveLocal, objectiveSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            var data = new SynchronizingData
            {
                UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync(),
                ObjectivesFilter = x => x.Title != ignore,
            };

            // Act.
            var synchronizationResult = await synchronizer.Synchronize(
                data,
                Connection.Object,
                new ConnectionInfoExternalDto(),
                new Progress<double>(),
                new CancellationTokenSource().Token);

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
        }

        [TestMethod]
        public async Task Synchronize_FilterRejectsLocalAndSynchronizedProject_SynchronizeRejectedToo()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            objectiveLocal.Title = "New value";
            objectiveRemote.UpdatedAt = DateTime.UtcNow.AddDays(-1);
            Fixture.Context.Objectives.Update(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            var data = new SynchronizingData
            {
                UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync(),
                ObjectivesFilter = x => x.ID != objectiveLocal.ID && x.ID != objectiveSynchronized.ID,
            };

            // Act.
            var synchronizationResult = await synchronizer.Synchronize(
                data,
                Connection.Object,
                new ConnectionInfoExternalDto(),
                new Progress<double>(),
                new CancellationTokenSource().Token);

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
        }

        [TestMethod]
        public async Task Synchronize_FilterRejectsRemoteObjective_SynchronizeRejectedToo()
        {
            // Arrange.
            var (objectiveLocal, _, objectiveRemote) = await ArrangeObjective();
            var ignore = "Ignore";
            objectiveLocal.Title = "New value";
            objectiveRemote.Title = ignore;
            objectiveRemote.UpdatedAt = DateTime.UtcNow.AddDays(-1);
            Fixture.Context.Objectives.Update(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            var data = new SynchronizingData
            {
                UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync(),
                ObjectivesFilter = x => x.Title != ignore,
            };

            // Act.
            var synchronizationResult = await synchronizer.Synchronize(
                data,
                Connection.Object,
                new ConnectionInfoExternalDto(),
                new Progress<double>(),
                new CancellationTokenSource().Token);

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
        }

        [TestMethod]
        public async Task Synchronize_FilterRejectsRemoteAndSynchronizedProject_SynchronizeRejectedToo()
        {
            // Arrange.
            var (objectiveLocal, _, objectiveRemote) = await ArrangeObjective();
            objectiveLocal.Title = "New value";
            objectiveRemote.UpdatedAt = DateTime.UtcNow.AddDays(-1);
            Fixture.Context.Objectives.Update(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            var data = new SynchronizingData
            {
                UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync(),
                ObjectivesFilter = x => x.ID == objectiveLocal.ID,
            };

            // Act.
            var synchronizationResult = await synchronizer.Synchronize(
                data,
                Connection.Object,
                new ConnectionInfoExternalDto(),
                new Progress<double>(),
                new CancellationTokenSource().Token);

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
        }

        [TestMethod]
        public async Task Synchronize_FilterRejectsAll_DoNotSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, objectiveRemote) = await ArrangeObjective();
            objectiveLocal.Title = "New value 1";
            objectiveRemote.Title = "New value 2";
            objectiveRemote.UpdatedAt = DateTime.UtcNow.AddDays(-1);
            Fixture.Context.Objectives.Update(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            var data = new SynchronizingData
            {
                UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync(),
                ObjectivesFilter = x => false,
            };

            // Act.
            var synchronizationResult = await synchronizer.Synchronize(
                data,
                Connection.Object,
                new ConnectionInfoExternalDto(),
                new Progress<double>(),
                new CancellationTokenSource().Token);

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
        }

        [TestMethod]
        public async Task Synchronize_LocalAndRemoteObjectiveSame_Synchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();

            objectiveRemote.Description = objectiveLocal.Description = "New same description";
            Fixture.Context.Objectives.Update(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            CheckObjectives(local, objectiveLocal);
            CheckObjectives(synchronized, Map(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        [DataRow(null)] // without author
        [DataRow(0)] // created by currently syncing user
        [DataRow(1)] // created by another user
        public async Task Synchronize_ObjectiveAddedLocal_AddObjectiveToRemoteAndSynchronize(int? authorNumber)
        {
            // Arrange.
            var objectiveLocal = MockData.DEFAULT_OBJECTIVES[0];
            objectiveLocal.Project = Project.local;
            objectiveLocal.ObjectiveType = await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();
            objectiveLocal.AuthorID = authorNumber.HasValue
                ? await Fixture.Context.Users.Skip(authorNumber.Value).Select(x => x.ID).FirstAsync()
                : null;

            MockRemoteObjectives(ArraySegment<ObjectiveExternalDto>.Empty);
            await Fixture.Context.Objectives.AddAsync(objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            await Fixture.Context.BimElementObjectives.AddAsync(
                new BimElementObjective
                {
                    BimElement = new BimElement
                    {
                        ParentName = "parent",
                        GlobalID = "guid",
                    },
                    Objective = objectiveLocal,
                });

            var dynamicField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            objectiveLocal.DynamicFields ??= new List<DynamicField>();
            objectiveLocal.DynamicFields.Add(dynamicField);
            await Fixture.Context.DynamicFields.AddAsync(dynamicField);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Add);
            CheckObjectives(objectiveLocal, local, false);
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        [DataRow(null)] // without author
        [DataRow(0)]    // created by currently syncing user
        [DataRow(1)]    // created by another user
        public async Task Synchronize_ObjectiveAddedRemote_AddObjectiveToLocalAndSynchronize(int? authorNumber)
        {
            // Arrange.
            var objectiveType = await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();
            var author = authorNumber.HasValue
                ? await Fixture.Context.Users.Skip(authorNumber.Value).Select(x => x.ExternalID).FirstOrDefaultAsync()
                : null;

            var objectiveRemote = new ObjectiveExternalDto
            {
                ExternalID = "external_id",
                ProjectExternalID = Project.remote.ExternalID,
                ObjectiveType = new ObjectiveTypeExternalDto { Name = objectiveType.Name },
                CreationDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow,
                Title = "Title",
                Description = "Description",
                AuthorExternalID = author,
                BimElements = new List<BimElementExternalDto>
                {
                    new BimElementExternalDto
                    {
                        GlobalID = "guid",
                        ParentName = "1.ifc",
                    },
                },
                Status = ObjectiveStatus.Open,
                UpdatedAt = DateTime.UtcNow,
            };
            MockRemoteObjectives(new[] { objectiveRemote });

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            CheckObjectives(synchronized, Map(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveRemovedFromLocal_RemoveObjectiveFromRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, _) = await ArrangeObjective();
            Fixture.Context.Remove(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (_, _, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Remove);
            await assertHelper.IsSynchronizedObjectivesCount(0);
            await assertHelper.IsLocalObjectivesCount(0);
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveRemovedFromRemote_RemoveObjectiveFromLocalAndSynchronize()
        {
            // Arrange.
            await ArrangeObjective(true);
            MockRemoteObjectives(ArraySegment<ObjectiveExternalDto>.Empty);

            // Act.
            var (_, _, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedObjectivesCount(0);
            await assertHelper.IsLocalObjectivesCount(0);
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveHasLocalChanges_ApplyLocalChangesToRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, _) = await ArrangeObjective();

            var description = objectiveLocal.Description = "New local description";
            Fixture.Context.Objectives.Update(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            CheckObjectives(local, objectiveLocal);
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            Assert.AreEqual(
                description,
                ResultObjectiveExternalDto.Description,
                "The remote description has not changed");
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveHasRemoteChanges_ApplyRemoteChangesToLocalAndSynchronize()
        {
            // Arrange.
            var (_, _, objectiveRemote) = await ArrangeObjective();
            var description = objectiveRemote.Description = "New remote description";

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            CheckObjectives(synchronized, Map(objectiveRemote), false);
            Assert.AreEqual(description, local.Description, "The local description has not changed");
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectivesHaveChanges_MergeObjectivesAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, objectiveRemote) = await ArrangeObjective();
            objectiveRemote.UpdatedAt = DateTime.UtcNow.AddDays(1);
            var title = objectiveLocal.Title = "New local title";
            var description = objectiveRemote.Description = "New remote description";
            var dueDateIrrelevant = objectiveLocal.DueDate = new DateTime(2021, 3, 10);
            var dueDateRelevant = objectiveRemote.DueDate = new DateTime(2021, 3, 11);
            Fixture.Context.Objectives.Update(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            Assert.AreEqual(
                title,
                synchronized.Title,
                "The synchronized project title does not match the expected value.");
            Assert.AreEqual(
                description,
                synchronized.Description,
                "The synchronized project description does not match the expected value.");
            Assert.AreNotEqual(
                dueDateIrrelevant,
                synchronized.DueDate,
                "The synchronized project due date has irrelevant value.");
            Assert.AreEqual(
                dueDateRelevant,
                synchronized.DueDate,
                "The synchronized project due date does not match the expected value.");
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalObjectiveHasNewItem_AddItemToRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, _) = await ArrangeObjective();
            var item = MockData.DEFAULT_ITEMS[0];
            item.Objectives ??= new List<ObjectiveItem>();
            item.Objectives.Add(
                new ObjectiveItem
                {
                    Item = item,
                    Objective = objectiveLocal,
                });
            await Fixture.Context.Items.AddAsync(item);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsSynchronizedItemsCount(1);
            await assertHelper.IsLocalItemsCount(1);
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_RemoteObjectiveHasNewItem_AddItemToLocalAndSynchronize()
        {
            // Arrange.
            var (_, _, remoteObjective) = await ArrangeObjective();
            remoteObjective.Items.Add(
                new ItemExternalDto
                {
                    ExternalID = "new_external_item_id",
                    RelativePath = "1.txt",
                    ItemType = ItemType.File,
                    UpdatedAt = DateTime.UtcNow,
                });

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedItemsCount(1);
            await assertHelper.IsLocalItemsCount(1);
            CheckObjectives(synchronized, Map(remoteObjective), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalAndRemoteObjectiveHaveNewItems_AddItemsAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, remoteObjective) = await ArrangeObjective();
            var item = MockData.DEFAULT_ITEMS[0];
            item.Objectives ??= new List<ObjectiveItem>();
            item.Objectives.Add(
                new ObjectiveItem
                {
                    Item = item,
                    Objective = objectiveLocal,
                });
            await Fixture.Context.Items.AddAsync(item);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            remoteObjective.Items.Add(
                new ItemExternalDto
                {
                    ExternalID = "new_external_item_id",
                    RelativePath = "1.txt",
                    ItemType = ItemType.File,
                    UpdatedAt = DateTime.UtcNow,
                });

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsSynchronizedItemsCount(2);
            await assertHelper.IsLocalItemsCount(2);
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ItemRemovedFromLocal_RemoveItemFromRemoteAndSynchronize()
        {
            // Arrange.
            var (_, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            var item = MockData.DEFAULT_ITEMS[0];
            var externalItem = new ItemExternalDto
            {
                ExternalID = item.ExternalID,
                RelativePath = "1.txt",
                ItemType = ItemType.File,
                UpdatedAt = DateTime.UtcNow,
            };
            objectiveRemote.Items.Add(externalItem);
            item.Objectives ??= new List<ObjectiveItem>();
            item.IsSynchronized = true;
            item.ExternalID = externalItem.ExternalID;
            item.Objectives.Add(
                new ObjectiveItem
                {
                    Item = item,
                    Objective = objectiveSynchronized,
                });
            await Fixture.Context.Items.AddAsync(item);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsSynchronizedItemsCount(0);
            await assertHelper.IsLocalItemsCount(0);
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ItemRemovedFromLocalObjectiveButStillExists_RemoveSynchronizedItemAndFromRemote()
        {
            // Arrange.
            var (_, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            var syncedItem = MockData.DEFAULT_ITEMS[0];
            var localItem = MockData.DEFAULT_ITEMS[0];
            var externalItem = new ItemExternalDto
            {
                ExternalID = syncedItem.ExternalID,
                RelativePath = "1.txt",
                ItemType = ItemType.File,
                UpdatedAt = DateTime.UtcNow,
            };
            localItem.SynchronizationMate = syncedItem;
            objectiveRemote.Items.Add(externalItem);
            syncedItem.Objectives ??= new List<ObjectiveItem>();
            syncedItem.IsSynchronized = true;
            syncedItem.ExternalID = externalItem.ExternalID;
            syncedItem.Objectives.Add(
                new ObjectiveItem
                {
                    Item = syncedItem,
                    Objective = objectiveSynchronized,
                });
            await Fixture.Context.Items.AddRangeAsync(syncedItem, localItem);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsSynchronizedItemsCount(0);
            await assertHelper.IsLocalItemsCount(0);
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ItemRemovedFromRemoteObjectiveAndItemIsNeeded_UnlinkLocalItemAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            var localItem = MockData.DEFAULT_ITEMS[0];
            var synchronizedItem = MockData.DEFAULT_ITEMS[0];
            synchronizedItem.Objectives ??= new List<ObjectiveItem>();
            synchronizedItem.IsSynchronized = true;
            synchronizedItem.Objectives.Add(
                new ObjectiveItem
                {
                    Item = synchronizedItem,
                    Objective = objectiveSynchronized,
                });
            localItem.Project = Project.local;
            localItem.Objectives ??= new List<ObjectiveItem>();
            localItem.Objectives.Add(
                new ObjectiveItem
                {
                    Item = localItem,
                    Objective = objectiveLocal,
                });
            await Fixture.Context.Items.AddRangeAsync(localItem, synchronizedItem);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize(true);

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedItemsCount(0);
            await assertHelper.IsLocalItemsCount(1);
            CheckObjectives(synchronized, Map(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ItemRemovedFromRemoteObjectiveAndItemIsNotNeeded_RemoveItemAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            var localItem = MockData.DEFAULT_ITEMS[0];
            var synchronizedItem = MockData.DEFAULT_ITEMS[0];
            synchronizedItem.Objectives ??= new List<ObjectiveItem>();
            synchronizedItem.IsSynchronized = true;
            synchronizedItem.Objectives.Add(
                new ObjectiveItem
                {
                    Item = synchronizedItem,
                    Objective = objectiveSynchronized,
                });
            localItem.Objectives ??= new List<ObjectiveItem>();
            localItem.Objectives.Add(
                new ObjectiveItem
                {
                    Item = localItem,
                    Objective = objectiveLocal,
                });
            await Fixture.Context.Items.AddRangeAsync(localItem, synchronizedItem);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedItemsCount(0);
            await assertHelper.IsLocalItemsCount(0);
            CheckObjectives(synchronized, Map(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalObjectiveHasNewBimElement_AddBimElementToRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, _) = await ArrangeObjective();
            objectiveLocal.BimElements ??= new List<BimElementObjective>();
            objectiveLocal.BimElements.Add(
                new BimElementObjective
                {
                    BimElement = new BimElement
                    {
                        GlobalID = "guid",
                        ParentName = "someIfc",
                    },
                });
            Fixture.Context.Objectives.Update(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsBimElementsCount(1);
            await assertHelper.IsBimElementObjectiveLinksCount(2);
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_RemoteObjectiveHasNewBimElement_AddBimElementToLocalAndSynchronize()
        {
            // Arrange.
            var (_, _, objectiveRemote) = await ArrangeObjective();
            objectiveRemote.BimElements ??= new List<BimElementExternalDto>();
            objectiveRemote.BimElements.Add(
                new BimElementExternalDto
                {
                    GlobalID = "guid",
                    ParentName = "1.ifc",
                });

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsBimElementsCount(1);
            await assertHelper.IsBimElementObjectiveLinksCount(2);
            CheckObjectives(synchronized, Map(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectivesHaveNewSameBimElements_SynchronizeBimElements()
        {
            // Arrange.
            var (objectiveLocal, _, objectiveRemote) = await ArrangeObjective();
            objectiveLocal.BimElements ??= new List<BimElementObjective>();
            var element = new BimElement
            {
                GlobalID = "guid",
                ParentName = "someIfc",
            };
            objectiveLocal.BimElements.Add(new BimElementObjective { BimElement = element });
            Fixture.Context.Objectives.Update(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            objectiveRemote.BimElements ??= new List<BimElementExternalDto>();
            objectiveRemote.BimElements.Add(mapper.Map<BimElementExternalDto>(element));

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsBimElementsCount(1);
            await assertHelper.IsBimElementObjectiveLinksCount(2);
            CheckObjectives(synchronized, Map(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectivesHaveNewBimElements_SynchronizeBimElements()
        {
            // Arrange.
            var (objectiveLocal, _, objectiveRemote) = await ArrangeObjective();
            objectiveLocal.BimElements ??= new List<BimElementObjective>();
            objectiveLocal.BimElements.Add(
                new BimElementObjective
                {
                    BimElement = new BimElement
                    {
                        GlobalID = "guid",
                        ParentName = "someIfc",
                    },
                });
            Fixture.Context.Objectives.Update(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            objectiveRemote.BimElements ??= new List<BimElementExternalDto>();
            objectiveRemote.BimElements.Add(new BimElementExternalDto
            {
                GlobalID = "external_global_id",
                ParentName = "external_parent_name",
            });

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsBimElementsCount(2);
            await assertHelper.IsBimElementObjectiveLinksCount(4);
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_BimElementRemovedFromLocalObjective_RemoveBimElementFromRemoteObjectiveAndSynchronize()
        {
            // Arrange.
            var (_, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            objectiveSynchronized.BimElements ??= new List<BimElementObjective>();
            var element = new BimElement
            {
                GlobalID = "guid",
                ParentName = "someIfc",
            };
            objectiveSynchronized.BimElements.Add(new BimElementObjective { BimElement = element });
            Fixture.Context.Objectives.Update(objectiveSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            objectiveRemote.BimElements ??= new List<BimElementExternalDto>();
            objectiveRemote.BimElements.Add(mapper.Map<BimElementExternalDto>(element));

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsBimElementsCount(0);
            await assertHelper.IsBimElementObjectiveLinksCount(0);
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_BimElementRemovedFromRemoteObjective_RemoveBimElementFromLocalObjectiveAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            objectiveLocal.BimElements ??= new List<BimElementObjective>();
            objectiveSynchronized.BimElements ??= new List<BimElementObjective>();
            var element = new BimElement
            {
                GlobalID = "guid",
                ParentName = "someIfc",
            };
            objectiveLocal.BimElements.Add(new BimElementObjective { BimElement = element });
            objectiveSynchronized.BimElements.Add(new BimElementObjective { BimElement = element });
            Fixture.Context.Objectives.Update(objectiveLocal);
            Fixture.Context.Objectives.Update(objectiveSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsBimElementsCount(0);
            await assertHelper.IsBimElementObjectiveLinksCount(0);
            CheckObjectives(synchronized, Map(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task
            Synchronize_LocalObjectiveHasNewDynamicFieldWithSubfield_AddDynamicFieldToRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, _) = await ArrangeObjective();
            objectiveLocal.DynamicFields ??= new List<DynamicField>();
            objectiveLocal.DynamicFields.Add(
                new DynamicField
                {
                    Name = "Big DF",
                    Type = DynamicFieldType.OBJECT.ToString(),
                    ChildrenDynamicFields = new List<DynamicField>
                    {
                        new DynamicField
                        {
                            Name = "Small DF",
                            Type = DynamicFieldType.STRING.ToString(),
                            Value = "value",
                        },
                    },
                });
            Fixture.Context.Objectives.Update(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsSynchronizedDynamicFieldsCount(2);
            await assertHelper.IsLocalDynamicFieldsCount(2);
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalObjectiveHasNewSubfield_AddDynamicFieldToRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            objectiveLocal.DynamicFields ??= new List<DynamicField>();
            objectiveSynchronized.DynamicFields ??= new List<DynamicField>();
            objectiveRemote.DynamicFields ??= new List<DynamicFieldExternalDto>();

            var localField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            localField.ChildrenDynamicFields.Add(MockData.DEFAULT_DYNAMIC_FIELDS[1]);
            localField.ExternalID = null;
            objectiveLocal.DynamicFields.Add(localField);

            var synchronizedField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            synchronizedField.IsSynchronized = true;
            localField.SynchronizationMate = synchronizedField;
            objectiveSynchronized.DynamicFields.Add(synchronizedField);

            var remoteField = new DynamicFieldExternalDto
            {
                ExternalID = "ex_field",
                Name = localField.Name,
                Value = localField.Value,
                Type = DynamicFieldType.DATE,
            };
            localField.ExternalID = synchronizedField.ExternalID = remoteField.ExternalID;
            objectiveRemote.DynamicFields.Add(remoteField);

            Fixture.Context.Objectives.UpdateRange(objectiveLocal, objectiveSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsSynchronizedDynamicFieldsCount(2);
            await assertHelper.IsLocalDynamicFieldsCount(2);
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_RemoteObjectiveHasNewDynamicField_AddDynamicFieldToLocalAndSynchronize()
        {
            // Arrange.
            var (_, _, objectiveRemote) = await ArrangeObjective();
            var field = mapper.Map<DynamicFieldExternalDto>(MockData.DEFAULT_DYNAMIC_FIELDS[0]);
            field.ExternalID = "ex_field";
            objectiveRemote.DynamicFields = new List<DynamicFieldExternalDto> { field };

            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedDynamicFieldsCount(1);
            await assertHelper.IsLocalDynamicFieldsCount(1);
            CheckObjectives(synchronized, Map(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_DynamicFieldsHaveChanges_MergeFieldsAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();

            var localField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            objectiveLocal.DynamicFields = new List<DynamicField> { localField };

            var synchronizedField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            synchronizedField.IsSynchronized = true;
            localField.SynchronizationMate = synchronizedField;
            objectiveSynchronized.DynamicFields = new List<DynamicField> { synchronizedField };

            var remoteField = new DynamicFieldExternalDto
            {
                ExternalID = "ex_field",
                Name = localField.Name,
                Value = localField.Value,
                Type = DynamicFieldType.DATE,
                UpdatedAt = DateTime.UtcNow.AddDays(1),
            };
            localField.ExternalID = synchronizedField.ExternalID = remoteField.ExternalID;
            objectiveRemote.DynamicFields = new List<DynamicFieldExternalDto> { remoteField };

            var newName = localField.Name = "New Name";
            var newValue = remoteField.Value = "New Value";
            var relevantType = (remoteField.Type = DynamicFieldType.FLOAT).ToString();
            var irrelevantType = localField.Type = DynamicFieldType.INTEGER.ToString();

            Fixture.Context.Objectives.UpdateRange(objectiveLocal, objectiveSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();
            var dynamicField = synchronized.DynamicFields.First();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsSynchronizedDynamicFieldsCount(1);
            await assertHelper.IsLocalDynamicFieldsCount(1);
            Assert.AreEqual(newName, dynamicField.Name, "The dynamic field name does not match the expected value.");
            Assert.AreEqual(newValue, dynamicField.Value, "The dynamic field value does not match the expected value.");
            Assert.AreEqual(
                relevantType,
                dynamicField.Type,
                "The dynamic field type does not match the expected value.");
            Assert.AreNotEqual(irrelevantType, dynamicField.Type, "The dynamic field type has irrelevant value");
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_DynamicFieldRemovedFromLocal_RemoveDynamicFieldFromRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();

            var synchronizedField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            synchronizedField.IsSynchronized = true;
            objectiveSynchronized.DynamicFields = new List<DynamicField> { synchronizedField };

            var remoteField = new DynamicFieldExternalDto
            {
                ExternalID = "ex_field",
                Name = synchronizedField.Name,
                Value = synchronizedField.Value,
                Type = DynamicFieldType.DATE,
                UpdatedAt = DateTime.UtcNow.AddDays(1),
            };
            synchronizedField.ExternalID = remoteField.ExternalID;
            objectiveRemote.DynamicFields = new List<DynamicFieldExternalDto> { remoteField };

            Fixture.Context.Objectives.UpdateRange(objectiveLocal, objectiveSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsSynchronizedDynamicFieldsCount(0);
            await assertHelper.IsLocalDynamicFieldsCount(0);
            CheckObjectives(synchronized, Map(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_DynamicFieldRemovedFromRemote_RemoveDynamicFieldFromLocalAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();

            var localField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            objectiveLocal.DynamicFields = new List<DynamicField> { localField };

            var synchronizedField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            synchronizedField.IsSynchronized = true;
            localField.SynchronizationMate = synchronizedField;
            objectiveSynchronized.DynamicFields = new List<DynamicField> { synchronizedField };

            localField.ExternalID = synchronizedField.ExternalID = "ex_field";

            Fixture.Context.Objectives.UpdateRange(objectiveLocal, objectiveSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedDynamicFieldsCount(0);
            await assertHelper.IsLocalDynamicFieldsCount(0);
            CheckObjectives(synchronized, Map(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveAddedLocalWithSubobjective_AddObjectivesToRemoteAndSynchronize()
        {
            // Arrange.
            var objectivesLocal = MockData.DEFAULT_OBJECTIVES.Take(2).ToArray();
            objectivesLocal[1].Project = objectivesLocal[0].Project = Project.local;
            objectivesLocal[1].ObjectiveType = objectivesLocal[0].ObjectiveType =
                await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();
            objectivesLocal[1].ParentObjective = objectivesLocal[0];

            MockRemoteObjectives(ArraySegment<ObjectiveExternalDto>.Empty);
            await Fixture.Context.Objectives.AddRangeAsync(objectivesLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (locals, synchronized, synchronizationResult) = await GetManyObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Add, Times.Exactly(2));
            CheckObjectives(locals.First(x => x.Description == objectivesLocal[0].Description), objectivesLocal[0], false);
            await assertHelper.IsSynchronizedObjectivesCount(2);
            await assertHelper.IsLocalObjectivesCount(2);
            Assert.AreEqual(
                synchronized.First(x => x.ParentObjectiveID == null).ExternalID,
                ResultObjectiveExternalDtos.First(x => x.ParentObjectiveExternalID != null).ParentObjectiveExternalID,
                "The ID of the remote child objective does not match the stored external ID of the parent objective.");
            CheckSynchronizedObjectives(
                locals.First(x => x.ParentObjectiveID == null),
                synchronized.First(x => x.ParentObjectiveID == null));
        }

        [TestMethod]
        public async Task Synchronize_SubobjectiveAddedLocal_AddObjectiveToRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, _) = await ArrangeObjective();
            var subobjective = CreateSubobjective(objectiveLocal);

            Fixture.Context.Objectives.Update(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (locals, synchronized, synchronizationResult) = await GetManyObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Add);
            CheckObjectives(locals.First(x => x.Description == subobjective.Description), subobjective, false);
            Assert.AreEqual(
                synchronized.First(x => x.ParentObjectiveID == null).ExternalID,
                ResultObjectiveExternalDto.ParentObjectiveExternalID,
                "The ID of the remote child objective does not match the stored external ID of the parent objective.");
            await assertHelper.IsSynchronizedObjectivesCount(2);
            await assertHelper.IsLocalObjectivesCount(2);
            CheckSynchronizedObjectives(
                locals.First(x => x.ParentObjectiveID == null),
                synchronized.First(x => x.ParentObjectiveID == null));
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveAddedRemoteWithSubobjective_AddObjectivesToLocalAndSynchronize()
        {
            // Arrange.
            var objectiveType = await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();

            var objectiveRemoteParent = new ObjectiveExternalDto
            {
                ExternalID = "external_id1",
                ProjectExternalID = Project.remote.ExternalID,
                ObjectiveType = new ObjectiveTypeExternalDto { Name = objectiveType.Name },
                Title = "Title1",
            };
            var objectiveRemoteChild = CreateSubobjective(objectiveRemoteParent);

            MockRemoteObjectives(new[] { objectiveRemoteChild, objectiveRemoteParent });

            // Act.
            var (locals, synchronized, synchronizationResult) = await GetManyObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedObjectivesCount(2);
            await assertHelper.IsLocalObjectivesCount(2);
            CheckObjectives(synchronized.First(x => x.ParentObjectiveID == null), Map(objectiveRemoteParent), false);
            CheckObjectives(synchronized.First(x => x.ParentObjectiveID != null), Map(objectiveRemoteChild), false);
            CheckSynchronizedObjectives(
                locals.First(x => x.ParentObjectiveID == null),
                synchronized.First(x => x.ParentObjectiveID == null));
        }

        [TestMethod]
        public async Task Synchronize_SubobjectiveAddedRemote_AddSubjectiveToLocalAndSynchronize()
        {
            // Arrange.
            var (_, _, objectiveRemote) = await ArrangeObjective();
            var objectiveRemoteChild = CreateSubobjective(objectiveRemote);
            MockRemoteObjectives(new[] { objectiveRemoteChild, objectiveRemote });

            // Act.
            var (locals, synchronized, synchronizationResult) = await GetManyObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedObjectivesCount(2);
            await assertHelper.IsLocalObjectivesCount(2);
            CheckObjectives(synchronized.First(x => x.ParentObjectiveID == null), Map(objectiveRemote), false);
            CheckObjectives(synchronized.First(x => x.ParentObjectiveID != null), Map(objectiveRemoteChild), false);
            CheckSynchronizedObjectives(
                locals.First(x => x.ParentObjectiveID == null),
                synchronized.First(x => x.ParentObjectiveID == null));
        }

        [TestMethod]
        public async Task Synchronize_SubobjectiveRemovedFromLocal_RemoveSubobjectiveFromRemoteAndSynchronize()
        {
            // Arrange.
            var (_, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            var subobjectiveSynchronized = CreateSubobjective(objectiveSynchronized);
            var objectiveRemoteChild = CreateSubobjective(objectiveRemote, subobjectiveSynchronized.ExternalID);

            MockRemoteObjectives(new[] { objectiveRemoteChild, objectiveRemote });
            Fixture.Context.Objectives.Update(objectiveSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Remove);
            await assertHelper.IsSynchronizedObjectivesCount(1);
            await assertHelper.IsLocalObjectivesCount(1);
            Assert.AreEqual(
                objectiveRemoteChild.ExternalID,
                ResultObjectiveExternalDto.ExternalID,
                "The ID of the removed objective incorrect");
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_SubobjectiveRemovedFromRemote_RemoveSubobjectiveFromLocalAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, _) = await ArrangeObjective();

            var subobjectiveLocal = CreateSubobjective(objectiveLocal);
            var subobjectiveSynchronized = CreateSubobjective(objectiveSynchronized, subobjectiveLocal.ExternalID);
            subobjectiveLocal.SynchronizationMate = subobjectiveSynchronized;
            var removingID = subobjectiveLocal.ExternalID;

            Fixture.Context.Objectives.UpdateRange(objectiveLocal, objectiveSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedObjectivesCount(1);
            await assertHelper.IsLocalObjectivesCount(1);
            Assert.AreNotEqual(removingID, local.ExternalID, "Local objective must be removed");
            Assert.AreNotEqual(removingID, synchronized.ExternalID, "Synchronized objective must be removed");
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ParentObjectiveRemovedFromLocal_RemoveParentAndChildFromRemote()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            var objectiveRemoteChild = CreateSubobjective(objectiveRemote);
            CreateSubobjective(objectiveSynchronized, objectiveRemoteChild.ExternalID);

            MockRemoteObjectives(new[] { objectiveRemoteChild, objectiveRemote });
            Fixture.Context.Objectives.Remove(objectiveLocal);
            Fixture.Context.Objectives.Update(objectiveSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var synchronizationResult = await Synchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(
                SynchronizerTestsHelper.SynchronizerCall.Remove,
                Times.Between(1, 2, Range.Inclusive));
            await assertHelper.IsSynchronizedObjectivesCount(0);
            await assertHelper.IsLocalObjectivesCount(0);
        }

        [TestMethod]
        public async Task Synchronize_ParentObjectiveRemovedFromRemote_RemoveParentAndChildFromLocal()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, _) = await ArrangeObjective();

            var subobjectiveLocal = CreateSubobjective(objectiveLocal);
            var subobjectiveSynchronized = CreateSubobjective(objectiveSynchronized, subobjectiveLocal.ExternalID);
            subobjectiveLocal.SynchronizationMate = subobjectiveSynchronized;

            Fixture.Context.Objectives.UpdateRange(objectiveLocal, objectiveSynchronized);
            MockRemoteObjectives(ArraySegment<ObjectiveExternalDto>.Empty);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var synchronizationResult = await Synchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedObjectivesCount(0);
            await assertHelper.IsLocalObjectivesCount(0);
        }

        private void CheckSynchronizedObjectives(Objective local, Objective synchronized)
        {
            SynchronizerTestsHelper.CheckSynchronized(local, synchronized);

            Assert.AreEqual(local.ExternalID, synchronized.ExternalID, "External IDs are not equal");

            if (local.ParentObjective != null || synchronized.ParentObjective != null)
            {
                Assert.AreEqual(
                    local.ParentObjective?.SynchronizationMateID,
                    synchronized.ParentObjectiveID,
                    "Parents are not synchronized");
            }

            Assert.AreEqual(
                local.ChildrenObjectives?.Count ?? 0,
                synchronized.ChildrenObjectives?.Count ?? 0,
                "Numbers of children are not equal");
            CheckObjectives(local, synchronized, false);

            foreach (var item in local.ChildrenObjectives ?? Enumerable.Empty<Objective>())
            {
                var synchronizedItem = synchronized.ChildrenObjectives?
                   .FirstOrDefault(x => item.SynchronizationMateID == x.ID);
                Assert.IsNotNull(synchronizedItem, "Cannot find synchronized child");
                CheckSynchronizedObjectives(item, synchronizedItem);
            }

            foreach (var item in local.Items ?? Enumerable.Empty<ObjectiveItem>())
            {
                var synchronizedItem = synchronized.Items?
                   .FirstOrDefault(x => item.Item.SynchronizationMateID == x.ItemID);
                Assert.IsNotNull(synchronizedItem, "Cannot find synchronized item");
                SynchronizerTestsHelper.CheckSynchronizedItems(item.Item, synchronizedItem.Item);
            }

            foreach (var item in local.DynamicFields ?? Enumerable.Empty<DynamicField>())
            {
                var synchronizedItem = synchronized.DynamicFields?
                   .FirstOrDefault(x => item.SynchronizationMateID == x.ID);
                Assert.IsNotNull(synchronizedItem, "Cannot find synchronized dynamic field");
                CheckSynchronizedDynamicFields(item, synchronizedItem);
            }
        }

        private void CheckObjectives(Objective a, Objective b, bool checkIDs = true)
        {
            Assert.AreEqual(a.Project.ExternalID, b.Project.ExternalID, "External IDs of projects are not equal");
            Assert.AreEqual(a.AuthorID, b.AuthorID, "Author IDs are not equal");
            Assert.AreEqual(a.ObjectiveType.Name, b.ObjectiveType.Name, "Objective types are not equal");
            Assert.AreEqual(a.CreationDate, b.CreationDate, "Creation dates are not equal");
            Assert.AreEqual(a.DueDate, b.DueDate, "Due dates are not equal");
            Assert.AreEqual(a.Title, b.Title, "Titles are not equal");
            Assert.AreEqual(a.Description, b.Description, "Descriptions are not equal");
            Assert.AreEqual(a.Status, b.Status, "Statuses are not equal");
            Assert.AreEqual(
                a.BimElements?.Count ?? 0,
                b.BimElements?.Count ?? 0,
                "Number of bim elements is not equal");
            Assert.AreEqual(a.Items?.Count ?? 0, b.Items?.Count ?? 0, "Numbers of items is not equal");
            Assert.AreEqual(
                a.DynamicFields?.Count ?? 0,
                b.DynamicFields?.Count ?? 0,
                "Numbers of dynamic fields is not equal");

            foreach (var bimElement in a.BimElements ?? Enumerable.Empty<BimElementObjective>())
            {
                var synchronizedElement = b.BimElements?.FirstOrDefault(
                    x => bimElement.BimElement.ParentName == x.BimElement.ParentName &&
                        bimElement.BimElement.GlobalID == x.BimElement.GlobalID);
                Assert.IsNotNull(synchronizedElement, "Cannot find synchronized bim element");
                CheckSynchronizedBimElements(bimElement.BimElement, bimElement.BimElement);
            }

            if (checkIDs)
            {
                SynchronizerTestsHelper.CheckIDs(a, b);
            }
        }

        private void CheckSynchronizedBimElements(BimElement local, BimElement synchronized)
        {
            Assert.AreEqual(local.ElementName, synchronized.ElementName, "Names of bim elements are not equal");
            Assert.AreEqual(local.ParentName, synchronized.ParentName, "Parent names of bim elements are not equal");
            Assert.AreEqual(local.GlobalID, synchronized.GlobalID, "Global IDs of bim elements are not equal");
        }

        private void CheckSynchronizedDynamicFields(DynamicField local, DynamicField synchronized)
        {
            Assert.AreEqual(local.Name, synchronized.Name, "Names of dynamic fields are not equal");
            Assert.AreEqual(local.Type, synchronized.Type, "Types of dynamic fields are not equal");
            Assert.AreEqual(local.Value, synchronized.Value, "Values of dynamic fields are not equal");

            SynchronizerTestsHelper.CheckSynchronized(local, synchronized);

            Assert.AreEqual(local.ChildrenDynamicFields?.Count ?? 0, synchronized.ChildrenDynamicFields?.Count ?? 0, "Number of bim elements is not equal");

            foreach (var item in local.ChildrenDynamicFields ?? Enumerable.Empty<DynamicField>())
            {
                var synchronizedItem = synchronized.ChildrenDynamicFields?
                   .FirstOrDefault(x => item.SynchronizationMateID == x.ID);
                Assert.IsNotNull(synchronizedItem, "Cannot find synchronized dynamic field child");
                CheckSynchronizedDynamicFields(item, synchronizedItem);
            }
        }

        private void CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall call, Times times = default)
            => SynchronizerTestsHelper.CheckSynchronizerCalls(ObjectiveSynchronizer, call, times);

        private async Task<(Objective local, Objective synchronized, ObjectiveExternalDto remote)> ArrangeObjective(bool dontSetupRemote = false)
        {
            var objectiveLocal = MockData.DEFAULT_OBJECTIVES[0];
            var objectiveSynchronized = MockData.DEFAULT_OBJECTIVES[0];
            var objectiveType = await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();
            var objectiveRemote = new ObjectiveExternalDto
            {
                ExternalID = "external_id",
                ProjectExternalID = Project.local.ExternalID,
                ParentObjectiveExternalID = string.Empty,
                AuthorExternalID = "author_external_id",
                ObjectiveType = new ObjectiveTypeExternalDto { Name = objectiveType.Name },
                UpdatedAt = DateTime.Now,
                CreationDate = objectiveLocal.CreationDate,
                DueDate = objectiveLocal.DueDate,
                Title = objectiveLocal.Title,
                Description = objectiveLocal.Description,
                Items = new List<ItemExternalDto>(),
                Status = (ObjectiveStatus)objectiveLocal.Status,
            };

            objectiveLocal.ExternalID = objectiveSynchronized.ExternalID = objectiveRemote.ExternalID;
            objectiveLocal.Project = Project.local;
            objectiveLocal.ProjectID = Project.local.ID;
            objectiveLocal.ObjectiveType = objectiveType;
            objectiveLocal.ObjectiveTypeID = objectiveType.ID;
            objectiveSynchronized.ObjectiveType = objectiveType;
            objectiveSynchronized.ObjectiveTypeID = objectiveType.ID;
            objectiveSynchronized.Project = Project.synchronized;
            objectiveSynchronized.ProjectID = Project.synchronized.ID;

            if (!dontSetupRemote)
                MockRemoteObjectives(new[] { objectiveRemote });

            objectiveSynchronized.IsSynchronized = true;
            objectiveLocal.SynchronizationMate = objectiveSynchronized;
            await Fixture.Context.Objectives.AddRangeAsync(objectiveSynchronized, objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            return (objectiveLocal, objectiveSynchronized, objectiveRemote);
        }

        private Objective CreateSubobjective(Objective parent, string externalId = "ex_subobjective")
        {
            var subobjective = MockData.DEFAULT_OBJECTIVES[1];
            parent.ChildrenObjectives = new List<Objective> { subobjective };
            subobjective.ProjectID = parent.ProjectID;
            subobjective.ObjectiveTypeID = parent.ObjectiveTypeID;
            subobjective.IsSynchronized = parent.IsSynchronized;
            subobjective.ExternalID = externalId;
            return subobjective;
        }

        private ObjectiveExternalDto CreateSubobjective(ObjectiveExternalDto parent, string externalId = "ex_subobjective")
        {
            var sample = MockData.DEFAULT_OBJECTIVES[1];
            var subobjective = new ObjectiveExternalDto
            {
                ExternalID = externalId,
                ProjectExternalID = parent.ProjectExternalID,
                ParentObjectiveExternalID = parent.ExternalID,
                AuthorExternalID = parent.AuthorExternalID,
                ObjectiveType = parent.ObjectiveType,
                UpdatedAt = DateTime.Now,
                CreationDate = sample.CreationDate,
                DueDate = sample.DueDate,
                Title = sample.Title,
                Description = sample.Description,
                Items = new List<ItemExternalDto>(),
                Status = (ObjectiveStatus)sample.Status,
            };

            return subobjective;
        }

        private async Task<
                (List<Objective> locals,
                List<Objective> synchronized,
                ICollection<SynchronizingResult> synchronizationResult)>
            GetManyObjectivesAfterSynchronize(bool ignoreProjects = false)
        {
            var synchronizationResult = await Synchronize(ignoreProjects);
            var locals = await SynchronizerTestsHelper.Include(Fixture.Context.Objectives.Unsynchronized())
               .ToListAsync();
            var synchronized = await SynchronizerTestsHelper.Include(Fixture.Context.Objectives.Synchronized())
               .ToListAsync();
            return (locals, synchronized, synchronizationResult);
        }

        private async
            Task<(Objective local, Objective synchronized, ICollection<SynchronizingResult> synchronizationResult)>
            GetObjectivesAfterSynchronize(bool ignoreProjects = false)
        {
            var synchronizationResult = await Synchronize(ignoreProjects);
            var local = await SynchronizerTestsHelper.Include(Fixture.Context.Objectives.Unsynchronized())
               .FirstOrDefaultAsync();
            var synchronized = await SynchronizerTestsHelper.Include(Fixture.Context.Objectives.Synchronized())
               .FirstOrDefaultAsync();
            return (local, synchronized, synchronizationResult);
        }
        
        private Objective Map(ObjectiveExternalDto objectiveExternalDto)
        {
            var obj = mapper.Map<Objective>(objectiveExternalDto);
            obj.Author = Fixture.Context.Users.Find(obj.AuthorID);
            obj.Project = Fixture.Context.Projects.Find(obj.ProjectID);
            return obj;
        }

        private async Task<ICollection<SynchronizingResult>> Synchronize(bool ignoreProjects = false)
        {
            var data = new SynchronizingData { UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync() };

            if (ignoreProjects)
                data.ProjectsFilter = x => false;

            var synchronizationResult = await synchronizer.Synchronize(
                data,
                Connection.Object,
                new ConnectionInfoExternalDto(),
                new Progress<double>(),
                new CancellationTokenSource().Token);
            return synchronizationResult;
        }

        private void MockRemoteObjectives(IReadOnlyCollection<ObjectiveExternalDto> array)
            => SynchronizerTestsHelper.MockGetRemote(ObjectiveSynchronizer, array, x => x.ExternalID);
    }
}
