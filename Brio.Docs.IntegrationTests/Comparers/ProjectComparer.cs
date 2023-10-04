using System.Diagnostics.CodeAnalysis;
using Brio.Docs.Client.Dtos;

namespace Brio.Docs.Tests
{
    internal class ProjectComparer : AbstractModelComparer<ProjectDto>
    {
        public ProjectComparer(bool ignoreIDs = false)
            : base(ignoreIDs)
        {
        }

        public override bool NotNullEquals([DisallowNull] ProjectDto x, [DisallowNull] ProjectDto y)
        {
            var dataEquals = x.Title == y.Title;
            if (!IgnoreIDs)
                dataEquals = dataEquals && x.ID == y.ID;
            return dataEquals;
        }
    }
}
