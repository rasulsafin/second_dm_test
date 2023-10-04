using System;
using AutoMapper;

namespace Brio.Docs.HttpConnection.Mapping
{
    internal class DtoMapper
    {
        private static readonly Lazy<Mapper> CONTAINER = new Lazy<Mapper>(CreateMapper);

        public static Mapper Instance => CONTAINER.Value;

        private static Mapper CreateMapper()
            => new Mapper(new MapperConfiguration(e => e.AddProfile(new AutoMapperProfile())));
    }
}
