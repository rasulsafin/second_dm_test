using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Tests.Utility;
using Brio.Docs.Utility.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Language;
using Moq.Language.Flow;

namespace Brio.Docs.Tests.Synchronization.Helpers
{
    internal class SynchronizerTestsHelper
    {
        public enum SynchronizerCall
        {
            Nothing,
            Add,
            Update,
            Remove,
        }

        public static void CheckSynchronizedItems(Item local, Item synchronized)
        {
            Assert.AreEqual(local.RelativePath, synchronized.RelativePath);
            Assert.AreEqual(local.Project?.SynchronizationMateID ?? 0, synchronized.Project?.ID ?? 0);
            Assert.AreEqual(local.SynchronizationMateID, synchronized.ID);
            Assert.IsFalse(local.IsSynchronized);
            Assert.IsTrue(synchronized.IsSynchronized);
            Assert.AreEqual(local.ItemType, synchronized.ItemType);
            Assert.AreEqual(local.ExternalID, synchronized.ExternalID);
        }

        public static void CheckIDs(ISynchronizableBase a, ISynchronizableBase b)
        {
            Assert.AreEqual(a.SynchronizationMateID, b.SynchronizationMateID);
            Assert.AreEqual(a.IsSynchronized, b.IsSynchronized);
        }

        public static void CheckSynchronized(ISynchronizableBase local, ISynchronizableBase synchronized)
        {
            Assert.AreEqual(local.SynchronizationMateID, synchronized.ID);
            Assert.IsFalse(local.IsSynchronized);
            Assert.IsTrue(synchronized.IsSynchronized);
        }

        public static async Task<(Project local, Project synchronized, ProjectExternalDto remote)> ArrangeProject(Mock<ISynchronizer<ProjectExternalDto>> projectSynchronizer, SharedDatabaseFixture fixture)
        {
            // Arrange.
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            var projectSynchronized = MockData.DEFAULT_PROJECTS[0];
            var projectRemote = new ProjectExternalDto
            {
                ExternalID = "external_id",
                Title = projectLocal.Title,
                UpdatedAt = DateTime.Now,
            };
            projectLocal.ExternalID = projectSynchronized.ExternalID = projectRemote.ExternalID;
            MockGetRemote(projectSynchronizer, new[] { projectRemote }, x => x.ExternalID);
            projectSynchronized.IsSynchronized = true;
            projectLocal.SynchronizationMate = projectSynchronized;
            await fixture.Context.Projects.AddRangeAsync(projectSynchronized, projectLocal);
            await fixture.Context.SaveChangesAsync();
            return (projectLocal, projectSynchronized, projectRemote);
        }

        public static void CheckSynchronizerCalls<T>(
            Mock<ISynchronizer<T>> synchronizer,
            SynchronizerCall call,
            Times times = default)
        {
            if (times == default)
                times = Times.Once();

            synchronizer.Verify(x => x.Add(It.IsAny<T>()), call == SynchronizerCall.Add ? times : Times.Never());
            synchronizer.Verify(x => x.Remove(It.IsAny<T>()), call == SynchronizerCall.Remove ? times : Times.Never());
            synchronizer.Verify(x => x.Update(It.IsAny<T>()), call == SynchronizerCall.Update ? times : Times.Never());
        }

        public static Mock<IConnectionContext> CreateConnectionContextStub(
            ISynchronizer<ProjectExternalDto> projectSynchronizer,
            ISynchronizer<ObjectiveExternalDto> objectiveSynchronizer)
        {
            var stub = new Mock<IConnectionContext>();
            stub.Setup(x => x.ObjectivesSynchronizer).Returns(objectiveSynchronizer);
            stub.Setup(x => x.ProjectsSynchronizer).Returns(projectSynchronizer);
            return stub;
        }

        public static SharedDatabaseFixture CreateFixture()
            => new SharedDatabaseFixture(
                context =>
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    var users = MockData.DEFAULT_USERS;
                    var objectiveTypes = MockData.DEFAULT_OBJECTIVE_TYPES;
                    context.Users.AddRange(users);
                    context.ObjectiveTypes.AddRange(objectiveTypes);
                    context.SaveChanges();
                });

        public static ServiceProvider CreateServiceProvider(DMContext context)
        {
            var services = new ServiceCollection();
            services.AddSingleton(context);
            services.AddSynchronizer();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            services.AddMappingResolvers();
            services.AddAutoMapper(typeof(MappingProfile));
            return services.BuildServiceProvider();
        }

        public static Mock<ISynchronizer<T>> CreateSynchronizerStub<T>(Action<T> callback = null)
            where T : class
        {
            void AddCallback(ICallback setup)
            {
                if (callback != null)
                    setup.Callback(callback);
            }

            var stub = new Mock<ISynchronizer<T>>();
            AddCallback(stub.Setup(x => x.Add(It.IsAny<T>())).Returns<T>(AddIDs));
            AddCallback(stub.Setup(x => x.Update(It.IsAny<T>())).Returns<T>(AddIDs));
            AddCallback(stub.Setup(x => x.Remove(It.IsAny<T>())).Returns<T>(AddIDs));
            return stub;
        }

        public static IQueryable<Objective> Include(IQueryable<Objective> set)
            => set
               .Include(x => x.DynamicFields)
               .Include(x => x.ObjectiveType)
               .Include(x => x.Project)
               .Include(x => x.Items)
               .ThenInclude(x => x.Item)
               .Include(x => x.ParentObjective)
               .ThenInclude(x => x.SynchronizationMate)
               .Include(x => x.Author)
               .Include(x => x.BimElements)
               .ThenInclude(x => x.BimElement)
               .Include(x => x.Location)
               .ThenInclude(x => x.Item);

        public static IQueryable<Project> Include(IQueryable<Project> set)
            => set
               .Include(x => x.SynchronizationMate)
               .Include(x => x.Users)
               .Include(x => x.Items);

        public static void MockGetRemote<T>(Mock<ISynchronizer<T>> synchronizer, IReadOnlyCollection<T> array, Func<T, string> getIDFunc)
        {
            synchronizer
               .Setup(x => x.GetUpdatedIDs(It.IsAny<DateTime>()))
               .ReturnsAsync(array.Select(getIDFunc).ToArray());
            synchronizer
               .Setup(x => x.Get(It.IsAny<IReadOnlyCollection<string>>()))
               .ReturnsAsync(array);
        }

        public static async ValueTask SaveChangesAndClearTracking(DbContext context)
        {
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        private static async Task<T> AddIDs<T>(T arg)
            where T : class
        {
            if (typeof(T) == typeof(ProjectExternalDto))
                return (await AddIDs(arg as ProjectExternalDto)) as T;

            if (typeof(T) == typeof(ObjectiveExternalDto))
                return (await AddIDs(arg as ObjectiveExternalDto)) as T;

            throw new NotSupportedException();
        }

        private static void AddIDs(DynamicFieldExternalDto df)
        {
            df.ExternalID ??= $"new_df_{Guid.NewGuid()}";

            foreach (var child in df.ChildrenDynamicFields)
                AddIDs(child);
        }

        private static Task<ObjectiveExternalDto> AddIDs(ObjectiveExternalDto x)
        {
            x.ExternalID ??= $"new_objective_{Guid.NewGuid()}";
            foreach (var item in x.Items)
                AddIDs(item);

            foreach (var df in x.DynamicFields)
                AddIDs(df);

            return Task.FromResult(x);
        }

        private static Task<ProjectExternalDto> AddIDs(ProjectExternalDto x)
        {
            x.ExternalID ??= $"new_project_{Guid.NewGuid()}";
            foreach (var item in x.Items)
                AddIDs(item);

            return Task.FromResult(x);
        }

        private static void AddIDs(ItemExternalDto x)
            => x.ExternalID ??= $"new_item_{Guid.NewGuid()}";
    }
}
