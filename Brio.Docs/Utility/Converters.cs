using AutoMapper;
using Brio.Docs.Client;

namespace Brio.Docs.Utility
{
    public class IDNullableIntTypeConverter<T> : ITypeConverter<ID<T>?, int?>
    {
        public int? Convert(ID<T>? source, int? destination, ResolutionContext context)
            => source.HasValue && source.Value.IsValid ? (int?)source.Value : null;
    }
}
