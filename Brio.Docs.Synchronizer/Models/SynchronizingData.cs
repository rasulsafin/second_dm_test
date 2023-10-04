using System;
using System.Linq.Expressions;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Synchronization.Models
{
    public class SynchronizingData
    {
        public Expression<Func<Objective, bool>> ObjectivesFilter { get; set; } = objective => true;

        public Expression<Func<Project, bool>> ProjectsFilter { get; set; } = project => true;

        public int UserId { get; set; }

        internal IConnectionContext ConnectionContext { get; set; }

        internal DateTime Date { get; set; }
    }
}
