using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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

namespace Brio.Docs.Tests.Synchronization
{
    [TestClass]
    public class SynchronizerProjectTests
    {
        private Synchronizer synchronizer;
        private IMapper mapper;
        private ServiceProvider serviceProvider;
        private AssertHelper assertHelper;

        private Mock<ISynchronizer<ObjectiveExternalDto>> ObjectiveSynchronizer { get; set; }

        private Mock<ISynchronizer<ProjectExternalDto>> ProjectSynchronizer { get; set; }

        private SharedDatabaseFixture Fixture { get; set; }

        private Mock<IConnection> Connection { get; set; }

        private Mock<IConnectionContext> Context { get; set; }

        private ProjectExternalDto ResultProjectExternalDto { get; set; }

        [TestInitialize]
        public void Setup()
        {
            Fixture = SynchronizerTestsHelper.CreateFixture();
            serviceProvider = SynchronizerTestsHelper.CreateServiceProvider(Fixture.Context);
            synchronizer = serviceProvider.GetService<Synchronizer>();
            mapper = serviceProvider.GetService<IMapper>();

            assertHelper = new AssertHelper(Fixture.Context);

            Connection = new Mock<IConnection>();
            ProjectSynchronizer =
                SynchronizerTestsHelper.CreateSynchronizerStub<ProjectExternalDto>(x => ResultProjectExternalDto = x);
            ObjectiveSynchronizer = new Mock<ISynchronizer<ObjectiveExternalDto>>();
            SynchronizerTestsHelper.MockGetRemote(
                ObjectiveSynchronizer,
                ArraySegment<ObjectiveExternalDto>.Empty,
                x => x.ExternalID);
            Context = SynchronizerTestsHelper.CreateConnectionContextStub(ProjectSynchronizer.Object, ObjectiveSynchronizer.Object);
            Connection.Setup(x => x.GetContext(It.IsAny<ConnectionInfoExternalDto>())).ReturnsAsync(Context.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Fixture.Dispose();
            serviceProvider.Dispose();
        }

        [TestMethod]
        public async Task Synchronize_ProjectUnchanged_DoNothing()
        {
            // Arrange.
            var (projectLocal, projectSynchronized, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            CheckProjects(local, projectLocal);
            CheckProjects(synchronized, projectSynchronized);
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalAndRemoteProjectsSame_Synchronize()
        {
            // Arrange.
            var (projectLocal, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            projectRemote.Title = projectLocal.Title = "New same title";
            Fixture.Context.Projects.Update(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            CheckProjects(local, projectLocal);
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_FilterRejectsLocalProject_SynchronizeRejectedToo()
        {
            // Arrange.
            var (projectLocal, _, _) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            var ignore = "Ignore";
            projectLocal.Title = ignore;
            Fixture.Context.Projects.Update(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            var data = new SynchronizingData
            {
                UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync(),
                ProjectsFilter = x => x.Title != ignore,
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
        public async Task Synchronize_FilterRejectsSynchronizedProject_SynchronizeRejectedToo()
        {
            // Arrange.
            var (projectLocal, projectSynchronized, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            var ignore = "Ignore";
            projectLocal.Title = "New value";
            projectRemote.UpdatedAt = DateTime.UtcNow.AddDays(-1);
            Fixture.Context.Projects.UpdateRange(projectLocal, projectSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            var data = new SynchronizingData
            {
                UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync(),
                ProjectsFilter = x => x.Title != ignore,
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
            var (projectLocal, projectSynchronized, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            projectLocal.Title = "New value";
            projectRemote.UpdatedAt = DateTime.UtcNow.AddDays(-1);
            Fixture.Context.Projects.Update(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            var data = new SynchronizingData
            {
                UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync(),
                ProjectsFilter = x => x.ID != projectLocal.ID && x.ID != projectSynchronized.ID,
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
        public async Task Synchronize_FilterRejectsRemoteProject_SynchronizeRejectedToo()
        {
            // Arrange.
            var (projectLocal, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            var ignore = "Ignore";
            projectLocal.Title = "New value";
            projectRemote.Title = ignore;
            projectRemote.UpdatedAt = DateTime.UtcNow.AddDays(-1);
            Fixture.Context.Projects.Update(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            var data = new SynchronizingData
            {
                UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync(),
                ProjectsFilter = x => x.Title != ignore,
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
            var (projectLocal, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            projectLocal.Title = "New value";
            projectRemote.UpdatedAt = DateTime.UtcNow.AddDays(-1);
            Fixture.Context.Projects.Update(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            var data = new SynchronizingData
            {
                UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync(),
                ProjectsFilter = x => x.ID == projectLocal.ID,
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
            var (projectLocal, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            projectLocal.Title = "New value 1";
            projectRemote.Title = "New value 2";
            projectRemote.UpdatedAt = DateTime.UtcNow.AddDays(-1);
            Fixture.Context.Projects.Update(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            var data = new SynchronizingData
            {
                UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync(),
                ProjectsFilter = x => false,
            };

            // Act.
            var synchronizationResult = await synchronizer.Synchronize(
                data,
                Connection.Object,
                new ConnectionInfoExternalDto(),
                new Progress<double>(),
                new CancellationTokenSource().Token);
            var local = await SynchronizerTestsHelper.Include(Fixture.Context.Projects.Unsynchronized()).FirstOrDefaultAsync();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            CheckProjects(projectLocal, local);
        }

        [TestMethod]
        public async Task Synchronize_ProjectAddedLocal_AddProjectToRemoteAndSynchronize()
        {
            // Arrange.
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            MockRemoteProjects(ArraySegment<ProjectExternalDto>.Empty);
            await Fixture.Context.Projects.AddAsync(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Add);
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectAddedLocalWithItems_AddProjectToRemoteAndSynchronize()
        {
            // Arrange.
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            var items = MockData.DEFAULT_ITEMS;
            projectLocal.Items = items;
            MockRemoteProjects(ArraySegment<ProjectExternalDto>.Empty);
            await Fixture.Context.Projects.AddAsync(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Add);
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
            Assert.AreEqual(
                items.Count,
                local.Items?.Count ?? 0,
                "The number of local items does not match the expected value");
            Assert.AreEqual(
                items.Count,
                ResultProjectExternalDto.Items?.Count ?? 0,
                "The number of remote items does not match the expected value");
        }

        [TestMethod]
        public async Task Synchronize_ProjectAddedRemote_AddProjectToLocalAndSynchronize()
        {
            // Arrange.
            var projectRemote = new ProjectExternalDto
            {
                ExternalID = "external_id",
                Title = "Title",
                UpdatedAt = DateTime.Now,
            };
            MockRemoteProjects(new[] { projectRemote });

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectRemovedLocal_RemoveProjectFromRemoteAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, _) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            Fixture.Context.Remove(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (_, _, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Remove);
            Assert.AreEqual(0, await Fixture.Context.Projects.Synchronized().CountAsync());
            Assert.AreEqual(0, await Fixture.Context.Projects.Synchronized().CountAsync());
        }

        [TestMethod]
        public async Task Synchronize_ProjectRemovedRemote_RemoveProjectFromLocalAndSynchronize()
        {
            // Arrange.
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            var projectSynchronized = MockData.DEFAULT_PROJECTS[0];
            projectLocal.ExternalID = projectSynchronized.ExternalID = "external_id";
            MockRemoteProjects(ArraySegment<ProjectExternalDto>.Empty);
            projectSynchronized.IsSynchronized = true;
            projectLocal.SynchronizationMate = projectSynchronized;
            await Fixture.Context.Projects.AddRangeAsync(projectSynchronized, projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (_, _, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(0, await Fixture.Context.Projects.Synchronized().CountAsync());
            Assert.AreEqual(0, await Fixture.Context.Projects.Synchronized().CountAsync());
        }

        [TestMethod]
        public async Task Synchronize_ProjectRenamedLocal_RenameRemoteProjectAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, _) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            projectLocal.Title = "New title";
            Fixture.Context.Projects.Update(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            CheckProjects(local, projectLocal);
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectRenamedRemote_RenameLocalProjectAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            var oldTitle = projectLocal.Title;
            projectRemote.Title = "New title";
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreNotEqual(oldTitle, local.Title, "The local project has not changed.");
            Assert.AreNotEqual(oldTitle, synchronized.Title, "The synchronized project has not changed.");
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectRenamedLocalThenRemote_RenameLocalProjectAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            projectLocal.Title = "Local title";
            var oldLocalTitle = projectLocal.Title;
            projectRemote.Title = "Remote title";
            projectRemote.UpdatedAt = DateTime.UtcNow.AddDays(1);
            var oldRemoteTitle = projectRemote.Title;
            Fixture.Context.Projects.Update(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreNotEqual(oldLocalTitle, local.Title, "The local project has not changed.");
            Assert.AreEqual(oldRemoteTitle, synchronized.Title, "The synchronized project has no remote changes.");
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectRenamedRemoteThenLocal_RenameRemoteProjectAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            projectLocal.Title = "Local title";
            var oldLocalTitle = projectLocal.Title;
            projectRemote.Title = "Remote title";
            projectRemote.UpdatedAt = DateTime.UtcNow.AddDays(-1);
            var oldRemoteTitle = projectRemote.Title;
            Fixture.Context.Projects.Update(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            Assert.AreEqual(oldLocalTitle, local.Title, "The local project has changed.");
            Assert.AreNotEqual(oldRemoteTitle, synchronized.Title, "The synchronized project has remote changes.");
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalProjectHasNewItem_AddItemToRemoteAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, _) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            var item = MockData.DEFAULT_ITEMS[0];
            item.Project = projectLocal;
            await Fixture.Context.Items.AddAsync(item);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsSynchronizedItemsCount(1);
            await assertHelper.IsLocalItemsCount(1);
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_RemoteProjectHasNewItem_AddItemToLocalAndSynchronize()
        {
            // Arrange.
            var (_, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            projectRemote.Items = new List<ItemExternalDto>
            {
                new ItemExternalDto
                {
                    ExternalID = "item_external_id",
                    RelativePath = "item_name",
                    ItemType = ItemType.File,
                    UpdatedAt = DateTime.UtcNow,
                },
            };

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedItemsCount(1);
            await assertHelper.IsLocalItemsCount(1);
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ItemLinkedToRemoteProjectAndItemRemovedFromLocalObjective_LinkItemToLocalAndSynchronize()
        {
            // Arrange.
            var (_, projectSynchronized, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);

            var itemLocal = MockData.DEFAULT_ITEMS[0];
            var itemSynchronized = MockData.DEFAULT_ITEMS[0];
            var objective = MockData.DEFAULT_OBJECTIVES[0];
            var objectiveType = MockData.DEFAULT_OBJECTIVE_TYPES[0];

            itemSynchronized.IsSynchronized = true;
            objective.IsSynchronized = true;

            itemLocal.SynchronizationMate = itemSynchronized;
            objective.Project = projectSynchronized;
            objective.ObjectiveType = objectiveType;
            itemSynchronized.Objectives = new List<ObjectiveItem>
            {
                new ObjectiveItem
                {
                    Objective = objective,
                },
            };

            projectRemote.Items = new List<ItemExternalDto>
            {
                new ItemExternalDto
                {
                    ExternalID = itemLocal.ExternalID,
                    RelativePath = "item_name",
                    ItemType = ItemType.File,
                    UpdatedAt = DateTime.UtcNow,
                },
            };

            await Fixture.Context.Objectives.AddAsync(objective);
            await Fixture.Context.Items.AddRangeAsync(itemLocal, itemSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize(true);

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedItemsCount(1);
            await assertHelper.IsLocalItemsCount(1);
            Assert.AreEqual(1, local.Items.Count, $"The local project has {local.Items.Count} items");
            Assert.AreEqual(1, synchronized.Items.Count, $"The synchronized project has {synchronized.Items.Count} items");
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
        }

        [TestMethod]
        public async Task Synchronize_LocalAndRemoteProjectsHaveNewItems_AddItemsAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            var item = MockData.DEFAULT_ITEMS[0];
            item.Project = projectLocal;
            await Fixture.Context.Items.AddAsync(item);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            projectRemote.Items = new List<ItemExternalDto>
            {
                new ItemExternalDto
                {
                    ExternalID = "item_external_id",
                    RelativePath = "item_name",
                    ItemType = ItemType.File,
                    UpdatedAt = DateTime.UtcNow,
                },
            };

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsSynchronizedItemsCount(2);
            await assertHelper.IsLocalItemsCount(2);
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ItemRemovedFromLocalProject_RemoveItemFromRemoteAndSynchronize()
        {
            // Arrange.
            var (_, projectSynchronized, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            var itemExternal = new ItemExternalDto
            {
                ExternalID = "item_external_id",
                RelativePath = "item_name",
                ItemType = ItemType.File,
                UpdatedAt = DateTime.UtcNow,
            };
            projectRemote.Items = new List<ItemExternalDto> { itemExternal };
            var item = MockData.DEFAULT_ITEMS[0];
            item.IsSynchronized = true;
            item.ProjectID = projectSynchronized.ID;
            item.ExternalID = itemExternal.ExternalID;
            await Fixture.Context.Items.AddAsync(item);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            await assertHelper.IsSynchronizedItemsCount(0);
            await assertHelper.IsLocalItemsCount(0);
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ItemRemovedFromRemoteProjectAndItemIsNeeded_UnlinkLocalItemAndSynchronize()
        {
            // Arrange.
            var (projectLocal, projectSynchronized, _) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            var item = MockData.DEFAULT_ITEMS[0];
            item.Project = projectLocal;
            item.Objectives ??= new List<ObjectiveItem>();
            var objective = MockData.DEFAULT_OBJECTIVES[0];
            item.Objectives.Add(new ObjectiveItem
            {
                Item = item,
                Objective = objective,
            });
            objective.Project = projectLocal;
            objective.ObjectiveType = await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();
            var itemSynchronized = MockData.DEFAULT_ITEMS[0];
            itemSynchronized.IsSynchronized = true;
            itemSynchronized.Project = projectSynchronized;
            await Fixture.Context.Items.AddAsync(item);
            await Fixture.Context.Items.AddAsync(itemSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, _, synchronizationResult) = await GetProjectsAfterSynchronize(true);

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedItemsCount(0);
            await assertHelper.IsLocalItemsCount(1);
            Assert.AreEqual(0, local.Items.Count, "The file is still linked to the project");
        }

        [TestMethod]
        public async Task Synchronize_ItemRemovedFromRemoteProjectAndItemIsNotNeeded_RemoveItemAndSynchronize()
        {
            // Arrange.
            var (projectLocal, projectSynchronized, _) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            var item = MockData.DEFAULT_ITEMS[0];
            item.Project = projectLocal;
            var itemSynchronized = MockData.DEFAULT_ITEMS[0];
            itemSynchronized.IsSynchronized = true;
            itemSynchronized.Project = projectSynchronized;
            await Fixture.Context.Items.AddAsync(item);
            await Fixture.Context.Items.AddAsync(itemSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (_, _, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            await assertHelper.IsSynchronizedItemsCount(0);
            await assertHelper.IsLocalItemsCount(0);
        }

        private async
            Task<(Project local, Project synchronized, ICollection<SynchronizingResult> synchronizationResult)>
            GetProjectsAfterSynchronize(bool ignoreObjectives = false)
        {
            var data = new SynchronizingData { UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync() };

            if (ignoreObjectives)
                data.ObjectivesFilter = x => false;

            var synchronizationResult = await synchronizer.Synchronize(
                data,
                Connection.Object,
                new ConnectionInfoExternalDto(),
                new Progress<double>(),
                new CancellationTokenSource().Token);

            var local = await SynchronizerTestsHelper.Include(Fixture.Context.Projects.Unsynchronized()).FirstOrDefaultAsync();
            var synchronized = await SynchronizerTestsHelper.Include(Fixture.Context.Projects.Synchronized()).FirstOrDefaultAsync();
            return (local, synchronized, synchronizationResult);
        }

        private void CheckProjects(Project a, Project b, bool checkIDs = true)
        {
            Assert.AreEqual(a.Title, b.Title, "The project title does not match the expected value.");

            if (checkIDs)
                SynchronizerTestsHelper.CheckIDs(a, b);
        }

        private void CheckSynchronizedProjects(Project local, Project synchronized)
        {
            CheckProjects(local, synchronized, false);

            SynchronizerTestsHelper.CheckSynchronized(local, synchronized);

            Assert.AreEqual(
                local.Items?.Count ?? 0,
                synchronized.Items?.Count ?? 0,
                "The number of project items is not equal.");
            Assert.AreEqual(
                local.Objectives?.Count ?? 0,
                synchronized.Objectives?.Count ?? 0,
                "The number of project objectives is not equal.");
            Assert.AreEqual(
                local.Users?.Count ?? 0,
                synchronized.Users?.Count ?? 0,
                "The number of project users is not equal.");
            Assert.AreEqual(local.ExternalID, synchronized.ExternalID, "The external IDs are not equal");

            foreach (var item in local.Items ?? Enumerable.Empty<Item>())
            {
                var synchronizedItem = synchronized.Items?
                   .FirstOrDefault(x => item.ExternalID == x.ExternalID || item.SynchronizationMateID == x.ID);
                SynchronizerTestsHelper.CheckSynchronizedItems(item, synchronizedItem);
            }
        }

        private void CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall call, Times times = default)
            => SynchronizerTestsHelper.CheckSynchronizerCalls(ProjectSynchronizer, call, times);

        private void MockRemoteProjects(IReadOnlyCollection<ProjectExternalDto> array)
            => SynchronizerTestsHelper.MockGetRemote(ProjectSynchronizer, array, x => x.ExternalID);
    }
}
