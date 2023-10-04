using System.Collections.Generic;
using AutoMapper;
using Brio.Docs.Client.Dtos;
using Brio.Docs.HttpConnection.Models;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.HttpConnection.Mapping.Converters
{
    internal class DynamicFieldDtoToModelObjectValueResolver : IValueResolver<DynamicFieldDto, ObjectDynamicField, object>
    {
        public object Resolve(DynamicFieldDto source, ObjectDynamicField destination, object destMember, ResolutionContext context)
        {
            var children = (source.Value as JArray).ToObject<ICollection<DynamicFieldDto>>();
            var result = DtoMapper.Instance.Map<ICollection<IDynamicField>>(children);
            return result;
        }
    }
}
