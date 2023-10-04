using System.Diagnostics.CodeAnalysis;
using Brio.Docs.Client.Dtos;

namespace Brio.Docs.Tests
{
    internal class DynamicFieldComparer : AbstractModelComparer<DynamicFieldDto>
    {
        public DynamicFieldComparer(bool ignoreIDs)
            : base(ignoreIDs)
        {
        }

        public override bool NotNullEquals([DisallowNull] DynamicFieldDto x, [DisallowNull] DynamicFieldDto y)
        {
            var idMatched = IgnoreIDs || x.ID == y.ID;
            var valueMatched = x.Value == y.Value && x.Value.GetType() == y.Value.GetType();

            return idMatched
                && valueMatched
                && x.Name == y.Name
                && x.Type == y.Type;
        }
    }
}
