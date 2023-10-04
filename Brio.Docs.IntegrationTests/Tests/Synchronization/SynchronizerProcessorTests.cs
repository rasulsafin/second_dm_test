using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Synchronization;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Tests.Synchronization.Helpers;
using Brio.Docs.Tests.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Brio.Docs.Tests.Synchronization
{
    [TestClass]
    public class SynchronizerProcessorTests
    {
        private DMContext context;
        private SynchronizingData defaultSynchronizingData;
        private SharedDatabaseFixture fixture;
        private SynchronizerProcessor processor;
        private User user;

        private Mock<ISynchronizationStrategy<Objective>> ObjectiveStrategyStub { get; } =
            new Mock<ISynchronizationStrategy<Objective>>();

        private Mock<ISynchronizationStrategy<Project>> ProjectStrategyStub { get; } =
            new Mock<ISynchronizationStrategy<Project>>();

        [TestInitialize]
        public async Task Setup()
        {
            fixture = SynchronizerTestsHelper.CreateFixture();
            context = fixture.Context;

            ProjectStrategyStub.Setup(strategy => strategy.Order(It.IsAny<IEnumerable<Project>>()))
                .Returns<IEnumerable<Project>>(enumeration => enumeration);
            ObjectiveStrategyStub.Setup(strategy => strategy.Order(It.IsAny<IEnumerable<Objective>>()))
                .Returns<IEnumerable<Objective>>(enumeration => enumeration);

            processor = new SynchronizerProcessor(
                context,
                Mock.Of<IAttacher<Project>>(),
                Mock.Of<IAttacher<Objective>>(),
                ProjectStrategyStub.Object,
                ObjectiveStrategyStub.Object,
                Mock.Of<ILogger<SynchronizerProcessor>>());

            context.Users.Add(
                new User
                {
                    Login = string.Empty,
                    Name = string.Empty,
                    PasswordHash = new byte[] { 1 },
                    PasswordSalt = new byte[] { 1 },
                });
            await context.SaveChangesAsync();
            user = await context.Users.FirstOrDefaultAsync();

            defaultSynchronizingData = new SynchronizingData
            {
                ObjectivesFilter = objective => true,
                ProjectsFilter = project => true,
                UserId = user.ID,
                ConnectionContext = null,
                Date = default,
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            fixture.Dispose();
        }

        [TestMethod]
        public async Task Synchronize_EmptySynchronizingProjectCollections_ContextTrackingIsEmpty()
        {
            // Arrange.
            var remoteCollection = new Project[] { };
            var localProjects = context.Set<Project>();

            // Act.
            await processor.Synchronize<Project, ProjectExternalDto>(
                defaultSynchronizingData,
                remoteCollection,
                localProjects,
                CancellationToken.None,
                new Progress<double>());

            var changeTracking = context.ChangeTracker.Entries();
            var countTracking = changeTracking.Count();

            // Assert.
            Assert.AreEqual(0, countTracking, "Change tracking must be empty after operation");
        }

        [TestMethod]
        public async Task Synchronize_UnsavableProjectEntityInProcess_ContextTrackingIsEmpty()
        {
            // Arrange.
            ProjectStrategyStub
                .Setup(
                    strategy => strategy.AddToLocal(
                        It.IsAny<SynchronizingTuple<Project>>(),
                        It.IsAny<SynchronizingData>(),
                        It.IsAny<CancellationToken>()))
                .Returns<SynchronizingTuple<Project>, SynchronizingData, CancellationToken>(
                    (tuple, data, token) =>
                    {
                        context.Projects.Add(
                            new Project
                            {
                                ID = 1,
                                SynchronizationMateID = -1,
                                Users = new List<UserProject>
                                {
                                    new UserProject { User = user },
                                },
                            });
                        return Task.FromResult<SynchronizingResult>(null);
                    });

            var remoteProjects = new[] { new Project { ExternalID = "projectExternalId" } };
            var localProjects = context.Set<Project>();

            // Act.
            var result = await processor.Synchronize<Project, ProjectExternalDto>(
                defaultSynchronizingData,
                remoteProjects,
                localProjects,
                CancellationToken.None,
                new Progress<double>());

            var changeTracking = context.ChangeTracker.Entries();
            var countTracking = changeTracking.Count();
            var countErrors = result.Count;
            var exception = result.FirstOrDefault()?.Exception;

            // Assert.
            Assert.AreNotEqual(0, countErrors, "Has no errors on synchronization. Test is only valid on unprocessable operation with DB");
            Assert.IsInstanceOfType(exception, typeof(DbUpdateException), "Arrange stage is invalid. Test is only valid on unprocessable operation with DB");
            Assert.AreEqual(0, countTracking, "Change tracking must be empty after operation");
        }

        [TestMethod]
        public async Task Synchronize_EmptySynchronizingObjectiveCollections_ContextTrackingIsEmpty()
        {
            // Arrange.
            var remoteCollection = new Objective[] { };
            var localObjectives = context.Set<Objective>();

            // Act.
            await processor.Synchronize<Objective, ObjectiveExternalDto>(
                defaultSynchronizingData,
                remoteCollection,
                localObjectives,
                CancellationToken.None,
                new Progress<double>());

            var changeTracking = context.ChangeTracker.Entries();
            var countTracking = changeTracking.Count();

            // Assert.
            Assert.AreEqual(0, countTracking, "Change tracking must be empty after operation");
        }

        [TestMethod]
        public async Task Synchronize_UnsavableObjectiveEntityInProcess_ContextTrackingIsEmpty()
        {
            // Arrange.
            ObjectiveStrategyStub
                .Setup(
                    strategy => strategy.AddToLocal(
                        It.IsAny<SynchronizingTuple<Objective>>(),
                        It.IsAny<SynchronizingData>(),
                        It.IsAny<CancellationToken>()))
                .Returns<SynchronizingTuple<Objective>, SynchronizingData, CancellationToken>(
                    (tuple, data, token) =>
                    {
                        context.Objectives.Add(
                            new Objective
                            {
                                Title = "Title",
                                ID = 1,
                                SynchronizationMateID = -1,
                                Author = user,
                            });
                        return Task.FromResult<SynchronizingResult>(null);
                    });

            var remoteObjectives = new[] { new Objective { ExternalID = "objectiveExternalId" } };
            var localObjectives = context.Set<Objective>();

            // Act.
            var result = await processor.Synchronize<Objective, ObjectiveExternalDto>(
                defaultSynchronizingData,
                remoteObjectives,
                localObjectives,
                CancellationToken.None,
                new Progress<double>());

            var changeTracking = context.ChangeTracker.Entries();
            var countTracking = changeTracking.Count();
            var countErrors = result.Count;
            var exception = result.FirstOrDefault()?.Exception;

            // Assert.
            Assert.AreNotEqual(0, countErrors, "Has no errors on synchronization. Test is only valid on unprocessable operation with DB");
            Assert.IsInstanceOfType(exception, typeof(DbUpdateException), "Arrange stage is invalid. Test is only valid on unprocessable operation with DB");
            Assert.AreEqual(0, countTracking, "Change tracking must be empty after operation");
        }
    }
}
