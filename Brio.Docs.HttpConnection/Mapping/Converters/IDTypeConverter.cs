using AutoMapper;
using Brio.Docs.Client;

namespace Brio.Docs.HttpConnection.Mapping.Converters
{
    internal class IDTypeConverter<TSource, TDestination> : ITypeConverter<ID<TSource>, ID<TDestination>>
    {
        public ID<TDestination> Convert(ID<TSource> source, ID<TDestination> destination, ResolutionContext context)
            => (ID<TDestination>)(int)source;
    }
}
