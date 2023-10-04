using System.Diagnostics.CodeAnalysis;
using Brio.Docs.Database.Models;

namespace Brio.Docs.Tests
{
    internal class ItemComparer : AbstractModelComparer<Item>
    {
        public ItemComparer(bool ignoreIDs = false)
            : base(ignoreIDs)
        {
        }

        public override bool NotNullEquals([DisallowNull] Item x, [DisallowNull] Item y)
        {
            var dataEquals = x.ItemType == y.ItemType && x.RelativePath == y.RelativePath;
            if (!IgnoreIDs)
                dataEquals = dataEquals && x.ID == y.ID;
            return dataEquals;
        }
    }
}
