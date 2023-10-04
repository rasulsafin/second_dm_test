using System;
using AutoMapper;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Common.Dtos;
using Brio.Docs.HttpConnection.Models;

namespace Brio.Docs.HttpConnection.Mapping.Converters
{
    internal class DynamicFieldDtoToModelTypeConverter : ITypeConverter<DynamicFieldDto, IDynamicField>
    {
        public IDynamicField Convert(DynamicFieldDto source, IDynamicField destination, ResolutionContext context)
        {
            switch (source.Type)
            {
                case DynamicFieldType.OBJECT:
                    return DtoMapper.Instance.Map<DynamicFieldDto, ObjectDynamicField>(source);
                case DynamicFieldType.BOOL:
                    return DtoMapper.Instance.Map<DynamicFieldDto, BoolField>(source);
                case DynamicFieldType.STRING:
                    return DtoMapper.Instance.Map<DynamicFieldDto, StringField>(source);
                case DynamicFieldType.INTEGER:
                    return DtoMapper.Instance.Map<DynamicFieldDto, IntField>(source);
                case DynamicFieldType.FLOAT:
                    return DtoMapper.Instance.Map<DynamicFieldDto, FloatField>(source);
                case DynamicFieldType.DATE:
                    return DtoMapper.Instance.Map<DynamicFieldDto, DateField>(source);
                case DynamicFieldType.ENUM:
                    return DtoMapper.Instance.Map<DynamicFieldDto, EnumerationField>(source);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
