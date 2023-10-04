using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

namespace Brio.Docs.Tests.Synchronization
{
    [TestClass]
    public class SynchronizerObjectiveLocationTests : IDisposable
    {
        private readonly Lazy<string> locationGuidDefault = new Lazy<string>(() => Guid.NewGuid().ToString());
        private AssertHelper assertHelper;
        private Mock<IConnection> connection;
        private SharedDatabaseFixture fixture;
        private Mock<ISynchronizer<ObjectiveExternalDto>> objectiveSynchronizer;
        private (Project local, Project synchronized, ProjectExternalDto remote) projects;
        private ObjectiveExternalDto remoteResult;
        private ServiceProvider serviceProvider;
        private Synchronizer synchronizer;

        [TestInitialize]
        public async Task Setup()
        {
            fixture = SynchronizerTestsHelper.CreateFixture();
            serviceProvider = SynchronizerTestsHelper.CreateServiceProvider(fixture.Context);
            synchronizer = serviceProvider.GetService<Synchronizer>();

            assertHelper = new AssertHelper(fixture.Context);

            connection = new Mock<IConnection>();
            var projectSynchronizer = SynchronizerTestsHelper.CreateSynchronizerStub<ProjectExternalDto>();
            objectiveSynchronizer =
                SynchronizerTestsHelper.CreateSynchronizerStub<ObjectiveExternalDto>(x => remoteResult = x);
            var context = SynchronizerTestsHelper.CreateConnectionContextStub(projectSynchronizer.Object, objectiveSynchronizer.Object);
            connection.Setup(x => x.GetContext(It.IsAny<ConnectionInfoExternalDto>())).ReturnsAsync(context.Object);
            projects = await SynchronizerTestsHelper.ArrangeProject(projectSynchronizer, fixture);
        }

        [TestCleanup]
        public void Cleanup()
            => Dispose();

        [TestMethod]
        public async Task Synchronize_NewLocalObjectiveWithoutLocation_AddObjectiveToRemoteWithoutLocation()
        {
            // Arrange.
            var objectiveLocal = await CreateDummyLocalObjective();
            MockRemoteObjectives(ArraySegment<ObjectiveExternalDto>.Empty);
            await fixture.Context.Objectives.AddAsync(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(fixture.Context);

            // Act.
            var synchronizationResult = await Synchronize();
            var synchronized = await GetSynchronized();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            AssertIsSynchronizedNotNull(synchronized);
            AssertIsRemotePushed(remoteResult);
            AssertIsSynchronizedLocationNull(synchronized);
            AssertIsLocationNotPushed(remoteResult);
        }

        [TestMethod]
        public async Task Synchronize_NewRemoteObjectiveWithoutLocation_AddObjectiveToLocalWithoutLocation()
        {
            // Arrange.
            var objectiveRemote = await CreateDummyRemoteObjective();
            MockRemoteObjectives(new[] { objectiveRemote });

            // Act.
            var synchronizationResult = await Synchronize();
            var local = await GetLocal();
            var synchronized = await GetSynchronized();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            AssertIsLocalNotNull(local);
            AssertIsSynchronizedNotNull(synchronized);
            AssertIsRemoteNotPushed(remoteResult);
            AssertIsLocalLocationNull(local);
            AssertIsSynchronizedLocationNull(synchronized);
        }

        [TestMethod]
        public async Task Synchronize_NewLocalObjectiveWithLocation_AddObjectiveToRemoteWithLocation()
        {
            // Arrange.
            var objectiveLocal = await CreateDummyLocalObjective();
            var item = GetItemExistingItem();
            objectiveLocal.Location = CreateLocation(item);

            MockRemoteObjectives(ArraySegment<ObjectiveExternalDto>.Empty);
            await fixture.Context.Objectives.AddAsync(objectiveLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(fixture.Context);

            // Act.
            var synchronizationResult = await Synchronize();
            var synchronized = await GetSynchronized();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            AssertIsSynchronizedNotNull(synchronized);
            AssertIsRemotePushed(remoteResult);
            AssertIsSynchronizedLocationNotNull(synchronized);
            AssertIsRemoteLocationNotNull(remoteResult);
            AssertLocation(objectiveLocal.Location, remoteResult.Location);
            AssertLocation(objectiveLocal.Location, synchronized.Location);
        }

        [TestMethod]
        public async Task Synchronize_LocationItemChangedFromRemote_ChangeLocationItemOnLocal()
        {
            // Arrange.
            var objectiveRemote = await CreateDummyRemoteObjective();
            var item1Local = GetItemExistingItem(isSynchronized: true);
            var item1Synchronized = item1Local.SynchronizationMate;
            var item2 = GetItemExistingItem(1);

            var objectiveLocal = await CreateDummyLocalObjective(true, objectiveRemote.ExternalID);
            objectiveLocal.Location = CreateLocation(item1Local);

            var objectiveSynchronized = objectiveLocal.SynchronizationMate;
            objectiveSynchronized.Location = CreateLocation(item1Synchronized);

            objectiveRemote.Location = CreateLocationDto(item2);

            MockRemoteObjectives(new[] { objectiveRemote });
            await fixture.Context.Items.AddRangeAsync(item1Local, item1Synchronized, item2);
            await fixture.Context.Objectives.AddRangeAsync(objectiveLocal, objectiveSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(fixture.Context);

            // Act.
            var synchronizationResult = await Synchronize();
            var local = await GetLocal();
            var synchronized = await GetSynchronized();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            AssertIsLocalNotNull(local);
            AssertIsSynchronizedNotNull(synchronized);
            AssertIsRemoteNotPushed(remoteResult);
            AssertIsLocalLocationNotNull(local);
            AssertIsSynchronizedLocationNotNull(synchronized);
            AssertLocationItem(objectiveRemote.Location, local.Location);
            AssertLocationItem(objectiveRemote.Location, synchronized.Location);
        }

        [TestMethod]
        public async Task Synchronize_LocationItemChangedFromLocal_ChangeLocationItemOnRemote()
        {
            // Arrange.
            var objectiveRemote = await CreateDummyRemoteObjective();
            var item1Local = GetItemExistingItem(isSynchronized: true);
            var item1Synchronized = item1Local.SynchronizationMate;
            var item2 = GetItemExistingItem(1);

            var objectiveLocal = await CreateDummyLocalObjective(true, objectiveRemote.ExternalID);
            objectiveLocal.Location = CreateLocation(item2);

            var objectiveSynchronized = objectiveLocal.SynchronizationMate;
            objectiveSynchronized.Location = CreateLocation(item1Synchronized);

            objectiveRemote.Location = CreateLocationDto(item1Synchronized);

            MockRemoteObjectives(new[] { objectiveRemote });
            await fixture.Context.Items.AddRangeAsync(item1Local, item1Synchronized, item2);
            await fixture.Context.Objectives.AddRangeAsync(objectiveLocal, objectiveSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(fixture.Context);

            // Act.
            var synchronizationResult = await Synchronize();
            var local = await GetLocal();
            var synchronized = await GetSynchronized();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            AssertIsLocalNotNull(local);
            AssertIsSynchronizedNotNull(synchronized);
            AssertIsRemotePushed(remoteResult);
            AssertIsLocalLocationNotNull(local);
            AssertIsSynchronizedLocationNotNull(synchronized);
            AssertIsRemoteLocationNotNull(remoteResult);
            AssertLocationItem(local.Location, synchronized.Location);
            AssertLocationItem(local.Location, remoteResult.Location);
        }

        [TestMethod]
        public async Task Synchronize_NewRemoteObjectiveWithLocation_AddObjectiveToLocalWithLocation()
        {
            // Arrange.
            var objectiveRemote = await CreateDummyRemoteObjective();
            var item = GetItemExistingItem();
            objectiveRemote.Location = CreateLocationDto(item);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(fixture.Context);
            MockRemoteObjectives(new[] { objectiveRemote });

            // Act.
            var synchronizationResult = await Synchronize();
            var local = await GetLocal();
            var synchronized = await GetSynchronized();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            AssertIsLocalNotNull(local);
            AssertIsSynchronizedNotNull(synchronized);
            AssertIsRemoteNotPushed(remoteResult);
            AssertIsLocalLocationNotNull(local);
            AssertIsSynchronizedLocationNotNull(synchronized);
            AssertLocation(objectiveRemote.Location, local.Location);
            AssertLocation(objectiveRemote.Location, synchronized.Location);
        }

        [TestMethod]
        public async Task Synchronize_RemoteLocationChanged_ChangeLocalLocation()
        {
            // Arrange.
            var objectiveRemote = await CreateDummyRemoteObjective();
            var itemLocal = GetItemExistingItem(isSynchronized: true);
            var itemSynchronized = itemLocal.SynchronizationMate;

            var objectiveLocal = await CreateDummyLocalObjective(true, objectiveRemote.ExternalID);
            objectiveLocal.Location = CreateLocation(itemLocal);

            var objectiveSynchronized = objectiveLocal.SynchronizationMate;
            objectiveSynchronized.Location = CreateLocation(itemSynchronized);

            objectiveRemote.Location = CreateLocationDto(itemLocal);
            objectiveRemote.Location.CameraPosition = (1.111, 2.454, -4666.22);

            MockRemoteObjectives(new[] { objectiveRemote });
            await fixture.Context.Items.AddRangeAsync(itemLocal, itemSynchronized);
            await fixture.Context.Objectives.AddRangeAsync(objectiveLocal, objectiveSynchronized);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(fixture.Context);

            // Act.
            var synchronizationResult = await Synchronize();
            var local = await GetLocal();
            var synchronized = await GetSynchronized();

            // Assert.
            assertHelper.IsSynchronizationSuccessful(synchronizationResult);
            AssertIsLocalNotNull(local);
            AssertIsSynchronizedNotNull(synchronized);
            AssertIsRemoteNotPushed(remoteResult);
            AssertIsLocalLocationNotNull(local);
            AssertIsSynchronizedLocationNotNull(synchronized);
            AssertLocation(objectiveRemote.Location, local.Location);
            AssertLocation(objectiveRemote.Location, synchronized.Location);
        }

        public void Dispose()
        {
            fixture.Dispose();
            serviceProvider.Dispose();
            GC.SuppressFinalize(this);
        }

        private static void AssertIsLocalLocationNotNull(Objective local)
            => Assert.IsNotNull(local.Location, "Local location is null");

        private static void AssertIsLocalLocationNull(Objective local)
            => Assert.IsNull(local.Location, "Local location is not null");

        private static void AssertIsLocalNotNull(Objective local)
            => Assert.IsNotNull(local, "The local objective is null");

        private static void AssertIsLocationNotPushed(ObjectiveExternalDto remote)
            => Assert.IsNull(remote.Location, "Location has been pushed");

        private static void AssertIsRemoteLocationNotNull(ObjectiveExternalDto objective)
            => Assert.IsNotNull(objective.Location, "Remote location is null");

        private static void AssertIsRemoteNotPushed(ObjectiveExternalDto objective)
            => Assert.IsNull(objective, "Remote objective has been pushed");

        private static void AssertIsRemotePushed(ObjectiveExternalDto objective)
            => Assert.IsNotNull(objective, "Remote objective has not been pushed");

        private static void AssertIsSynchronizedLocationNotNull(Objective synchronized)
            => Assert.IsNotNull(synchronized.Location, "Synchronized location is null");

        private static void AssertIsSynchronizedLocationNull(Objective synchronized)
            => Assert.IsNull(synchronized.Location, "Synchronized location is not null");

        private static void AssertIsSynchronizedNotNull(Objective synchronized)
            => Assert.IsNotNull(synchronized, "The synchronized objective is null");

        private static void AssertLocation(Location expected, Location actual)
        {
            Assert.AreEqual(expected.Guid, actual.Guid, "The location GUID does not match the expected value.");
            Assert.AreEqual(expected.PositionX, actual.PositionX, "The location Position X does not match the expected value.");
            Assert.AreEqual(expected.PositionY, actual.PositionY, "The location Position Y does not match the expected value.");
            Assert.AreEqual(expected.PositionZ, actual.PositionZ, "The location Position Z does not match the expected value.");
            Assert.AreEqual(expected.CameraPositionX, actual.CameraPositionX, "The location Camera Position X does not match the expected value.");
            Assert.AreEqual(expected.CameraPositionY, actual.CameraPositionY, "The location Camera Position Y does not match the expected value.");
            Assert.AreEqual(expected.CameraPositionZ, actual.CameraPositionZ, "The location Camera Position Z does not match the expected value.");
            AssertLocationItem(expected, actual);
        }

        private static void AssertLocation(Location expected, LocationExternalDto actual)
        {
            Assert.AreEqual(expected.Guid, actual.Guid, "The location GUID does not match the expected value.");
            Assert.AreEqual(expected.PositionX, actual.Location.x, "The location X does not match the expected value.");
            Assert.AreEqual(expected.PositionY, actual.Location.y, "The location Y does not match the expected value.");
            Assert.AreEqual(expected.PositionZ, actual.Location.z, "The location Z does not match the expected value.");
            Assert.AreEqual(expected.CameraPositionX, actual.CameraPosition.x, "The location Camera Position X does not match the expected value.");
            Assert.AreEqual(expected.CameraPositionY, actual.CameraPosition.y, "The location Camera Position Y does not match the expected value.");
            Assert.AreEqual(expected.CameraPositionZ, actual.CameraPosition.z, "The location Camera Position Z does not match the expected value.");
            AssertLocationItem(expected, actual);
        }

        private static void AssertLocation(LocationExternalDto expected, Location actual)
        {
            Assert.AreEqual(expected.Guid, actual.Guid, "The location GUID does not match the expected value.");
            Assert.AreEqual(expected.Location.x, actual.PositionX, "The location Position X does not match the expected value.");
            Assert.AreEqual(expected.Location.y, actual.PositionY, "The location Position Y does not match the expected value.");
            Assert.AreEqual(expected.Location.z, actual.PositionZ, "The location Position Z does not match the expected value.");
            Assert.AreEqual(expected.CameraPosition.x, actual.CameraPositionX, "The location Camera Position X does not match the expected value.");
            Assert.AreEqual(expected.CameraPosition.y, actual.CameraPositionY, "The location Camera Position Y does not match the expected value.");
            Assert.AreEqual(expected.CameraPosition.z, actual.CameraPositionZ, "The location Camera Position Z does not match the expected value.");
            AssertLocationItem(expected, actual);
        }

        private static void AssertLocation(LocationExternalDto expected, LocationExternalDto actual)
        {
            Assert.AreEqual(expected.Guid, actual.Guid, "The location GUID does not match the expected value.");
            Assert.AreEqual(expected.Location.x, actual.Location.x, "The location X does not match the expected value.");
            Assert.AreEqual(expected.Location.y, actual.Location.y, "The location Y does not match the expected value.");
            Assert.AreEqual(expected.Location.z, actual.Location.z, "The location Z does not match the expected value.");
            Assert.AreEqual(expected.CameraPosition.x, actual.CameraPosition.x, "The location Camera Position X does not match the expected value.");
            Assert.AreEqual(expected.CameraPosition.y, actual.CameraPosition.y, "The location Camera Position Y does not match the expected value.");
            Assert.AreEqual(expected.CameraPosition.z, actual.CameraPosition.z, "The location Camera Position Z does not match the expected value.");
            AssertLocationItem(expected, actual);
        }

        private static void AssertLocationItem(Location expected, Location actual)
        {
            Assert.IsNotNull(actual.Item, "Actual item is null");
            Assert.AreEqual(expected.Item.ExternalID, actual.Item.ExternalID, "The location item does not match the expected value.");
        }

        private static void AssertLocationItem(Location expected, LocationExternalDto actual)
        {
            Assert.IsNotNull(actual.Item, "Actual item is null");
            Assert.AreEqual(expected.Item.ExternalID, actual.Item.ExternalID, "The location item does not match the expected value.");
        }

        private static void AssertLocationItem(LocationExternalDto expected, Location actual)
        {
            Assert.IsNotNull(actual.Item, "Actual item is null");
            Assert.AreEqual(expected.Item.ExternalID, actual.Item.ExternalID, "The location item does not match the expected value.");
        }

        private static void AssertLocationItem(LocationExternalDto expected, LocationExternalDto actual)
        {
            Assert.IsNotNull(actual.Item, "Actual item is null");
            Assert.AreEqual(expected.Item.ExternalID, actual.Item.ExternalID, "The location item does not match the expected value.");
        }

        private void CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall call, Times times = default)
            => SynchronizerTestsHelper.CheckSynchronizerCalls(objectiveSynchronizer, call, times);

        private async Task<Objective> CreateDummyLocalObjective(bool isSynchronized = false, string externalId = null)
        {
            var objectiveLocal = await CreateDummyObjective(externalId);
            objectiveLocal.Project = projects.local;

            if (isSynchronized)
            {
                var synchronized = CreateDummySynchronizedObjective(externalId);
                objectiveLocal.SynchronizationMate = await synchronized;
            }

            return objectiveLocal;
        }

        private async Task<Objective> CreateDummyObjective(string externalId)
        {
            var objectiveSynchronized = MockData.DEFAULT_OBJECTIVES[0];
            objectiveSynchronized.ObjectiveType = await fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();
            objectiveSynchronized.ExternalID = externalId;
            return objectiveSynchronized;
        }

        private async Task<ObjectiveExternalDto> CreateDummyRemoteObjective()
        {
            var objectiveType = await fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();

            var objectiveRemote = new ObjectiveExternalDto
            {
                ExternalID = "external_id",
                ProjectExternalID = projects.remote.ExternalID,
                ObjectiveType = new ObjectiveTypeExternalDto { Name = objectiveType.Name },
                CreationDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow,
                Title = "Title",
                Description = "Description",
                BimElements = new List<BimElementExternalDto>(),
                Status = ObjectiveStatus.Open,
                UpdatedAt = DateTime.UtcNow,
            };
            return objectiveRemote;
        }

        private async Task<Objective> CreateDummySynchronizedObjective(string externalId = null)
        {
            var objectiveSynchronized = await CreateDummyObjective(externalId);
            objectiveSynchronized.Project = projects.synchronized;
            objectiveSynchronized.IsSynchronized = true;
            return objectiveSynchronized;
        }

        private Location CreateLocation(Item item)
            => new Location
            {
                PositionX = 1.29,
                PositionY = -222.5,
                PositionZ = 0.0001,
                CameraPositionX = 0.000,
                CameraPositionY = 0.111,
                CameraPositionZ = 0.2323,
                Guid = locationGuidDefault.Value,
                Item = item,
            };

        private LocationExternalDto CreateLocationDto(Item item)
            => new LocationExternalDto
            {
                Location = (1.29, -222.5, 0.0001),
                CameraPosition = (0.000, 0.111, 0.2323),
                Guid = locationGuidDefault.Value,
                Item = new ItemExternalDto
                {
                    ExternalID = item.ExternalID,
                    RelativePath = item.Name,
                },
            };

        private Item GetItemExistingItem(int index = 0, bool isSynchronized = false)
        {
            projects.local.Items ??= new List<Item>();
            var item = MockData.DEFAULT_ITEMS[index];
            projects.local.Items.Add(item);

            if (isSynchronized)
            {
                projects.synchronized.Items ??= new List<Item>();
                var itemSynchronized = MockData.DEFAULT_ITEMS[index];
                projects.synchronized.Items.Add(itemSynchronized);
                itemSynchronized.IsSynchronized = true;
                item.SynchronizationMate = itemSynchronized;

                projects.remote.Items ??= new List<ItemExternalDto>();
                projects.remote.Items.Add(
                    new ItemExternalDto
                    {
                        ExternalID = itemSynchronized.ExternalID,
                        RelativePath = itemSynchronized.RelativePath,
                        ItemType = ItemType.File,
                        UpdatedAt = itemSynchronized.UpdatedAt,
                    });
            }

            return item;
        }

        private async Task<Objective> GetLocal()
            => await SynchronizerTestsHelper.Include(fixture.Context.Objectives.Unsynchronized())
               .FirstOrDefaultAsync();

        private async Task<Objective> GetSynchronized()
            => await SynchronizerTestsHelper.Include(fixture.Context.Objectives.Synchronized())
               .FirstOrDefaultAsync();

        private void MockRemoteObjectives(IReadOnlyCollection<ObjectiveExternalDto> array)
            => SynchronizerTestsHelper.MockGetRemote(objectiveSynchronizer, array, x => x.ExternalID);

        private async Task<ICollection<SynchronizingResult>> Synchronize()
            => await synchronizer.Synchronize(
                new SynchronizingData
                {
                    UserId = await fixture.Context.Users.Select(x => x.ID).FirstAsync(),
                },
                connection.Object,
                new ConnectionInfoExternalDto(),
                new Progress<double>(),
                CancellationToken.None);
    }
}
