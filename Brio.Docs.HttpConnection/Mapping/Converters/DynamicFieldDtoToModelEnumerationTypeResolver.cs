using AutoMapper;
using Brio.Docs.Client.Dtos;
using Brio.Docs.HttpConnection.Mapping.Extensions;
using Brio.Docs.HttpConnection.Models;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.HttpConnection.Mapping.Converters
{
    internal class DynamicFieldDtoToModelEnumerationTypeResolver : IValueResolver<DynamicFieldDto, IDynamicField, EnumerationType>
    {
        public EnumerationType Resolve(DynamicFieldDto source, IDynamicField destination, EnumerationType destMember, ResolutionContext context)
        {
            var enumerationValue = (source.Value as JObject).ToObject<Enumeration>();
            return enumerationValue.EnumerationType.ToModel();
        }
    }
}
