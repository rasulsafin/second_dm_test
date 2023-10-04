using System;
using System.IO;
using System.Linq;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Filters;
using Brio.Docs.Client.Sorts;
using Brio.Docs.Common;
using Brio.Docs.HttpConnection.Mapping.Converters;
using Brio.Docs.HttpConnection.Mapping.Extensions;
using Brio.Docs.HttpConnection.Models;

namespace Brio.Docs.HttpConnection.Mapping
{
    internal class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMapIDDtoToID();
            CreateMapIDToIDDto();
            CreateMapModelToDtoNew();
            CreateMapModelToDto();
            CreateMapDtoToModel();
        }

        private void CreateMapDtoToModel()
        {
            CreateMap<BimElementDto, BimElement>();
            CreateMap<BimElementStatusDto, BimElementStatus>();

            CreateMap<DynamicFieldDto, IDynamicField>()
                .ConvertUsing<DynamicFieldDtoToModelTypeConverter>();
            CreateMap<DynamicFieldDto, ObjectDynamicField>()
                .ForMember(d => d.Value, o => o.MapFrom<DynamicFieldDtoToModelObjectValueResolver>());
            CreateMap<DynamicFieldDto, EnumerationField>()
                .ForMember(d => d.Value, o => o.MapFrom<DynamicFieldDtoToModelEnumerationValueResolver>())
                .ForMember(d => d.EnumerationType, o => o.MapFrom<DynamicFieldDtoToModelEnumerationTypeResolver>());
            CreateMap<DynamicFieldDto, BoolField>();
            CreateMap<DynamicFieldDto, DateField>();
            CreateMap<DynamicFieldDto, FloatField>();
            CreateMap<DynamicFieldDto, IntField>();
            CreateMap<DynamicFieldDto, StringField>();

            CreateMap<ItemDto, Item>()
                .ForMember(d => d.FileName, o => o.MapFrom(s => Path.GetFileName(s.RelativePath)));
            CreateMap<ObjectiveDto, Objective>();
            CreateMap<ObjectiveTypeDto, ObjectiveType>();
            CreateMap<UserDto, User>();
            CreateMap<ObjectiveToListDto, Objective>();
            CreateMap<ObjectiveToSelectionDto, ObjectiveSelection>();
            CreateMap<ProjectDto, Project>()
                    .ForMember(d => d.Items, o => o.Ignore());
            CreateMap<ProjectToListDto, Project>()
                 .ForMember(d => d.ID, s => s.MapFrom(a => a.ID.Convert<ProjectDto, Project>()));
            CreateMap<EnumerationValueDto, EnumerationValue>();
            CreateMap<EnumerationTypeDto, EnumerationType>();
            CreateMap<LocationDto, Location>()
               .ForMember(d => d.Position, s => s.MapFrom(o => new Vector3d(o.Position.x, o.Position.y, o.Position.z)))
               .ForMember(
                    d => d.CameraPosition,
                    s => s.MapFrom(o => new Vector3d(o.CameraPosition.x, o.CameraPosition.y, o.CameraPosition.z)));

            CreateMap<PagedDataDto, PagedData>().ConstructUsing(dto => new PagedData(dto.CurrentPage, dto.PageSize, dto.TotalPages, dto.TotalCount));
            CreateMap<ObjectiveToLocationDto, Objective>();
            CreateMap<SubobjectiveDto, Objective>();
        }

        private void CreateMapModelToDto()
        {
            CreateMap<BimElement, BimElementDto>();

            CreateMap<IDynamicField, DynamicFieldDto>()
               .ForMember(d => d.Value, o => o.MapFrom<DynamicFieldToDtoValueResolver>());
            CreateMap<ObjectDynamicField, DynamicFieldDto>();
            CreateMap<BoolField, DynamicFieldDto>();
            CreateMap<DateField, DynamicFieldDto>();
            CreateMap<FloatField, DynamicFieldDto>();
            CreateMap<IntField, DynamicFieldDto>();
            CreateMap<StringField, DynamicFieldDto>();
            CreateMap<EnumerationField, DynamicFieldDto>()
                .ForMember(d => d.Value, o => o.MapFrom<DynamicFieldToDtoValueResolver>());

            CreateMap<Item, ItemDto>();
            CreateMap<Objective, ObjectiveDto>()
                    .ForMember(d => d.BimElements, o => o.MapFrom(x => x.BimElements.Select(el => el.ToDto())))
                    .ForMember(d => d.DynamicFields, o => o.MapFrom(x => x.DynamicFields.Select(f => f.ToDto())));
            CreateMap<Objective, ObjectiveToCreateDto>()
               .ForMember(d => d.AuthorID, s => s.MapFrom(a => a.AuthorID.Convert<User, UserDto>()))
               .ForMember(d => d.ProjectID, s => s.MapFrom(a => a.ProjectID.Convert<Project, ProjectDto>()))
               .ForMember(d => d.ObjectiveTypeID, s => s.MapFrom(a => a.ObjectiveType.ID.Convert<ObjectiveType, ObjectiveTypeDto>()));
            CreateMap<ObjectiveType, ObjectiveTypeDto>();
            CreateMap<Project, ProjectDto>();
            CreateMap<Project, ProjectToCreateDto>();
            CreateMap<User, UserDto>();

            CreateMap<EnumerationType, EnumerationTypeDto>();
            CreateMap<EnumerationValue, EnumerationValueDto>();

            CreateMap<Location, LocationDto>()
               .ForMember(
                    d => d.Position,
                    s => s.MapFrom(
                        o => new Tuple<double, double, double>(o.Position.X, o.Position.Y, o.Position.Z).ToValueTuple()))
               .ForMember(
                    d => d.CameraPosition,
                    s => s.MapFrom(
                        o => new Tuple<double, double, double>(o.CameraPosition.X, o.CameraPosition.Y, o.CameraPosition.Z)
                           .ToValueTuple()));

            CreateMap<ObjectivesFilter, ObjectiveFilterParameters>();
            CreateMap<ObjectivesSort, SortParameters>();
            CreateMap<ObjectivesSortParameter, SortParameter>();
        }

        private void CreateMapModelToDtoNew()
        {
            CreateMap<Item, ItemToCreateDto>();
            CreateMap<Objective, ObjectiveToCreateDto>()
                    .ForMember(d => d.BimElements, o => o.MapFrom(x => x.BimElements.Select(el => el.ToDto())))
                    .ForMember(d => d.ObjectiveTypeID, o => o.MapFrom(x => x.ObjectiveType.ID));
            CreateMap<User, UserToCreateDto>();
        }

        private void CreateMapIDToIDDto()
        {
            CreateMap<ID<ItemDto>, ID<Item>>().ConvertUsing<IDTypeConverter<ItemDto, Item>>();
            CreateMap<ID<ObjectiveDto>, ID<Objective>>().ConvertUsing<IDTypeConverter<ObjectiveDto, Objective>>();
            CreateMap<ID<ProjectDto>, ID<Project>>().ConvertUsing<IDTypeConverter<ProjectDto, Project>>();
            CreateMap<ID<UserDto>, ID<User>>().ConvertUsing<IDTypeConverter<UserDto, User>>();
            CreateMap<ID<BimElementDto>, ID<BimElement>>().ConvertUsing<IDTypeConverter<BimElementDto, BimElement>>();
            CreateMap<ID<DynamicFieldDto>, ID<IDynamicField>>()
                    .ConvertUsing<IDTypeConverter<DynamicFieldDto, IDynamicField>>();
            CreateMap<ID<ObjectiveTypeDto>, ID<ObjectiveType>>()
                    .ConvertUsing<IDTypeConverter<ObjectiveTypeDto, ObjectiveType>>();
            CreateMap<ID<ConnectionTypeDto>, ID<ConnectionType>>()
                    .ConvertUsing<IDTypeConverter<ConnectionTypeDto, ConnectionType>>();
            CreateMap<ID<EnumerationValueDto>, ID<EnumerationValue>>()
                    .ConvertUsing<IDTypeConverter<EnumerationValueDto, EnumerationValue>>();
            CreateMap<ID<EnumerationTypeDto>, ID<EnumerationType>>()
                    .ConvertUsing<IDTypeConverter<EnumerationTypeDto, EnumerationType>>();
        }

        private void CreateMapIDDtoToID()
        {
            CreateMap<ID<Item>, ID<ItemDto>>().ConvertUsing<IDTypeConverter<Item, ItemDto>>();
            CreateMap<ID<Objective>, ID<ObjectiveDto>>().ConvertUsing<IDTypeConverter<Objective, ObjectiveDto>>();
            CreateMap<ID<Project>, ID<ProjectDto>>().ConvertUsing<IDTypeConverter<Project, ProjectDto>>();
            CreateMap<ID<User>, ID<UserDto>>().ConvertUsing<IDTypeConverter<User, UserDto>>();
            CreateMap<ID<BimElement>, ID<BimElementDto>>().ConvertUsing<IDTypeConverter<BimElement, BimElementDto>>();
            CreateMap<ID<IDynamicField>, ID<DynamicFieldDto>>()
                    .ConvertUsing<IDTypeConverter<IDynamicField, DynamicFieldDto>>();
            CreateMap<ID<ObjectiveType>, ID<ObjectiveTypeDto>>()
                    .ConvertUsing<IDTypeConverter<ObjectiveType, ObjectiveTypeDto>>();
            CreateMap<ID<ConnectionType>, ID<ConnectionTypeDto>>()
                    .ConvertUsing<IDTypeConverter<ConnectionType, ConnectionTypeDto>>();
            CreateMap<ID<EnumerationValue>, ID<EnumerationValueDto>>()
                .ConvertUsing<IDTypeConverter<EnumerationValue, EnumerationValueDto>>();
            CreateMap<ID<EnumerationType>, ID<EnumerationTypeDto>>()
                    .ConvertUsing<IDTypeConverter<EnumerationType, EnumerationTypeDto>>();
        }
    }
}
