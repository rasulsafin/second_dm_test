using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public class SynchronizerFailProjectTests
    {
        private Synchronizer synchronizer;
        private ServiceProvider serviceProvider;

        private Mock<ISynchronizer<ObjectiveExternalDto>> ObjectiveSynchronizer { get; set; }

        private Mock<ISynchronizer<ProjectExternalDto>> ProjectSynchronizer { get; set; }

        private SharedDatabaseFixture Fixture { get; set; }

        private Mock<IConnection> Connection { get; set; }

        [TestInitialize]
        public void Setup()
        {
            Fixture = SynchronizerTestsHelper.CreateFixture();
            serviceProvider = SynchronizerTestsHelper.CreateServiceProvider(Fixture.Context);
            synchronizer = serviceProvider.GetService<Synchronizer>();

            Connection = new Mock<IConnection>();

            ProjectSynchronizer = new Mock<ISynchronizer<ProjectExternalDto>>();
            ObjectiveSynchronizer = new Mock<ISynchronizer<ObjectiveExternalDto>>();

            ObjectiveSynchronizer.Setup(x => x.Get(It.IsAny<List<string>>()))
               .ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Fixture.Dispose();
            serviceProvider.Dispose();
        }

        [TestMethod]
        public async Task SynchronizeFail_ProjectAddedLocal_ButContextFail_DoNothing()
        {
            // Arrange.
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            ProjectSynchronizer
               .Setup(x => x.GetUpdatedIDs(It.IsAny<DateTime>()))
               .ReturnsAsync(new[] { "id" });
            ProjectSynchronizer
               .Setup(x => x.Get(It.IsAny<IReadOnlyCollection<string>>()))
               .Throws(new Exception());
            await Fixture.Context.Projects.AddAsync(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);

            // Act.
            var (local, synchronized, result) = await SynchronizingResults();

            // Assert.
            CheckSynchronizer();
            CheckProjects(local, projectLocal);
            Assert.IsNull(synchronized);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public async Task SynchronizeFail_ProjectAddedLocal_ButCouldNotAddToRemote_DoNothing()
        {
            // Arrange.
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            ProjectSynchronizer
               .Setup(x => x.GetUpdatedIDs(It.IsAny<DateTime>()))
               .ReturnsAsync(ArraySegment<string>.Empty);
            ProjectSynchronizer
               .Setup(x => x.Get(It.IsAny<IReadOnlyCollection<string>>()))
               .ReturnsAsync(ArraySegment<ProjectExternalDto>.Empty);
            await Fixture.Context.Projects.AddAsync(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            ProjectSynchronizer.Setup(x => x.Add(It.IsAny<ProjectExternalDto>()))
               .Throws(new Exception());

            // Act.
            var (local, synchronized, result) = await SynchronizingResults();

            // Assert.
            CheckSynchronizer();
            CheckProjects(local, projectLocal);
            Assert.IsNull(synchronized);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public async Task SynchronizeFail_ProjectRemovedLocal_ButCouldNotRemoveFromRemote_DoNothing()
        {
            // Arrange.
            var (projectLocal, _, _) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            Fixture.Context.Remove(projectLocal);
            await SynchronizerTestsHelper.SaveChangesAndClearTracking(Fixture.Context);
            ProjectSynchronizer.Setup(x => x.Add(It.IsAny<ProjectExternalDto>()))
                .Throws(new Exception());

            // Act.
            await SynchronizingResults();

            // Assert.
            CheckSynchronizer();
            Assert.AreEqual(1, await Fixture.Context.Projects.Synchronized().CountAsync());
            Assert.AreEqual(1, await Fixture.Context.Projects.Synchronized().CountAsync());
        }

        private async Task<(Project local, Project synchronized, ICollection<SynchronizingResult> result)> SynchronizingResults()
        {
            var result = await synchronizer.Synchronize(
                new SynchronizingData { UserId = await Fixture.Context.Users.Select(x => x.ID).FirstAsync() },
                Connection.Object,
                new ConnectionInfoExternalDto(),
                new Progress<double>(),
                new CancellationTokenSource().Token);
            var local = await Fixture.Context.Projects.Unsynchronized().FirstOrDefaultAsync();
            var synchronized = await Fixture.Context.Projects.Synchronized().FirstOrDefaultAsync();
            return (local, synchronized, result);
        }

        private void CheckProjects(Project a, Project b, bool checkIDs = true)
        {
            Assert.AreEqual(a.Title, b.Title);

            if (checkIDs)
                SynchronizerTestsHelper.CheckIDs(a, b);
        }

        private void CheckSynchronizer()
        {
            ProjectSynchronizer.Verify(x => x.Add(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Remove(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(It.IsAny<ProjectExternalDto>()), Times.Never);
        }
    }
}
