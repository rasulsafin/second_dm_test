using AutoMapper;
using Brio.Docs.Client.Dtos;
using Brio.Docs.HttpConnection.Mapping.Extensions;
using Brio.Docs.HttpConnection.Models;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.HttpConnection.Mapping.Converters
{
    internal class DynamicFieldDtoToModelEnumerationValueResolver : IValueResolver<DynamicFieldDto, IDynamicField, object>
    {
        public object Resolve(DynamicFieldDto source, IDynamicField destination, object destMember, ResolutionContext context)
        {
            var enumerationValue = (source.Value as JObject).ToObject<Enumeration>();
            return enumerationValue.Value.ToModel();
        }
    }
}
