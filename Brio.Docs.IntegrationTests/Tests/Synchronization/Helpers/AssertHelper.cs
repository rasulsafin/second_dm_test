using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Tests.Synchronization.Helpers
{
    internal class AssertHelper
    {
        private readonly DMContext context;

        public AssertHelper(DMContext context)
            => this.context = context;

        public async ValueTask IsBimElementObjectiveLinksCount(int count)
            => Assert.AreEqual(
                count,
                await context.BimElementObjectives.CountAsync(),
                $"The number of 'bim element <-> objective' links  is not equal to {count}");

        public async ValueTask IsBimElementsCount(int count)
            => Assert.AreEqual(
                count,
                await context.BimElements.CountAsync(),
                $"The number of bim elements is not equal to {count}");

        public async ValueTask IsLocalDynamicFieldsCount(int count)
            => Assert.AreEqual(
                count,
                await context.DynamicFields.Unsynchronized().CountAsync(),
                $"The number of local dynamic fields is not equal to {count}");

        public async ValueTask IsLocalItemsCount(int count)
            => Assert.AreEqual(
                count,
                await context.Items.Unsynchronized().CountAsync(),
                $"The number of local items is not equal to {count}");

        public async ValueTask IsLocalObjectivesCount(int count)
            => Assert.AreEqual(
                count,
                await context.Objectives.Unsynchronized().CountAsync(),
                $"The number of local objectives is not equal to {count}");

        public void IsSynchronizationSuccessful(ICollection<SynchronizingResult> synchronizingResult)
            => Assert.AreEqual(
                0,
                synchronizingResult.Count,
                $"The synchronization failed with {synchronizingResult.Count} exceptions");

        public async ValueTask IsSynchronizedDynamicFieldsCount(int count)
            => Assert.AreEqual(
                count,
                await context.DynamicFields.Synchronized().CountAsync(),
                $"The number of synchronized dynamic fields is not equal to {count}");

        public async ValueTask IsSynchronizedItemsCount(int count)
            => Assert.AreEqual(
                count,
                await context.Items.Synchronized().CountAsync(),
                $"The number of synchronized items is not equal to {count}");

        public async ValueTask IsSynchronizedObjectivesCount(int count)
            => Assert.AreEqual(
                count,
                await context.Objectives.Synchronized().CountAsync(),
                $"The number of synchronized objectives is not equal to {count}");
    }
}
