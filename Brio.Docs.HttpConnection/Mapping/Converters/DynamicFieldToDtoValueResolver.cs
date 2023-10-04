using AutoMapper;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Common.Dtos;
using Brio.Docs.HttpConnection.Mapping.Extensions;
using Brio.Docs.HttpConnection.Models;

namespace Brio.Docs.HttpConnection.Mapping.Converters
{
    internal class DynamicFieldToDtoValueResolver : IValueResolver<IDynamicField, DynamicFieldDto, object>
    {
        public object Resolve(IDynamicField source, DynamicFieldDto destination, object destMember, ResolutionContext context)
        {
            if (source.Type != DynamicFieldType.ENUM)
                return source.Value;

            var enumeration = new Enumeration
            {
                EnumerationType = (source as EnumerationField).EnumerationType.ToDto(),
                Value = (source.Value as EnumerationValue).ToDto(),
            };

            return enumeration;
        }
    }
}
