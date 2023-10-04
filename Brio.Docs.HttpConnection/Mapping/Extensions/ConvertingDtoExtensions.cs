using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Filters;
using Brio.Docs.Client.Sorts;
using Brio.Docs.HttpConnection.Models;

namespace Brio.Docs.HttpConnection.Mapping.Extensions
{
    public static class ConvertingDtoExtensions
    {
        public static ID<TDestination> Convert<TSource, TDestination>(this ID<TSource> obj)
            => DtoMapper.Instance.Map<ID<TDestination>>(obj);

        public static ID<TDestination> Convert<TSource, TDestination>(this ID<TSource>? obj)
            => obj == null ? default : DtoMapper.Instance.Map<ID<TDestination>>(obj);

        #region ToCreate

        public static ItemToCreateDto ToCreateDto(this Item obj)
            => DtoMapper.Instance.Map<ItemToCreateDto>(obj);

        public static ObjectiveToCreateDto ToCreateDto(this Objective obj)
            => DtoMapper.Instance.Map<ObjectiveToCreateDto>(obj);

        public static ProjectToCreateDto ToCreateDto(this Project obj)
            => DtoMapper.Instance.Map<ProjectToCreateDto>(obj);

        public static UserToCreateDto ToCreateDto(this User obj)
            => DtoMapper.Instance.Map<UserToCreateDto>(obj);

        #endregion

        #region ToDto

        public static BimElementDto ToDto(this BimElement obj)
            => DtoMapper.Instance.Map<BimElementDto>(obj);

        public static DynamicFieldDto ToDto(this IDynamicField obj)
            => DtoMapper.Instance.Map<DynamicFieldDto>(obj);

        public static ItemDto ToDto(this Item obj)
            => DtoMapper.Instance.Map<ItemDto>(obj);

        public static ObjectiveDto ToDto(this Objective obj)
            => DtoMapper.Instance.Map<ObjectiveDto>(obj);

        public static ObjectiveTypeDto ToDto(this ObjectiveType obj)
            => DtoMapper.Instance.Map<ObjectiveTypeDto>(obj);

        public static ProjectDto ToDto(this Project obj)
            => DtoMapper.Instance.Map<ProjectDto>(obj);

        public static UserDto ToDto(this User obj)
            => DtoMapper.Instance.Map<UserDto>(obj);

        public static EnumerationValueDto ToDto(this EnumerationValue obj)
            => DtoMapper.Instance.Map<EnumerationValueDto>(obj);

        public static EnumerationTypeDto ToDto(this EnumerationType obj)
             => DtoMapper.Instance.Map<EnumerationTypeDto>(obj);

        public static ObjectiveFilterParameters ToDto(this ObjectivesFilter obj, int pageNumber, int pageSize)
        {
            var ret = DtoMapper.Instance.Map<ObjectiveFilterParameters>(obj);
            ret.PageNumber = pageNumber;
            ret.PageSize = pageSize;
            return ret;
        }

        public static ObjectiveFilterParameters ToDto(this ObjectivesFilter obj)
            => DtoMapper.Instance.Map<ObjectiveFilterParameters>(obj);

        public static SortParameters ToDto(this ObjectivesSort obj)
            => DtoMapper.Instance.Map<SortParameters>(obj);

        #endregion

        #region ToModel

        public static EnumerationType ToModel(this EnumerationTypeDto obj)
            => DtoMapper.Instance.Map<EnumerationType>(obj);

        public static BimElement ToModel(this BimElementDto obj)
            => DtoMapper.Instance.Map<BimElement>(obj);

        public static IDynamicField ToModel(this DynamicFieldDto obj)
            => DtoMapper.Instance.Map<IDynamicField>(obj);

        public static EnumerationValue ToModel(this EnumerationValueDto obj)
            => DtoMapper.Instance.Map<EnumerationValue>(obj);

        public static Item ToModel(this ItemDto obj)
            => DtoMapper.Instance.Map<Item>(obj);

        public static Objective ToModel(this ObjectiveDto obj, ObjectiveTypeDto objectiveType)
        {
            var model = new Objective
            {
                ObjectiveType = objectiveType.ToModel(),
            };

            model = DtoMapper.Instance.Map(obj, model);
            return model;
        }

        public static ObjectiveType ToModel(this ObjectiveTypeDto obj)
            => DtoMapper.Instance.Map<ObjectiveType>(obj);

        public static Project ToModel(this ProjectDto obj)
            => DtoMapper.Instance.Map<Project>(obj);

        public static User ToModel(this UserDto obj)
            => DtoMapper.Instance.Map<User>(obj);

        public static Objective ToModel(this ObjectiveToListDto objectiveToListDto)
            => DtoMapper.Instance.Map<Objective>(objectiveToListDto);

        public static ObjectiveSelection ToModel(this ObjectiveToSelectionDto objectiveToSelectionDto)
            => DtoMapper.Instance.Map<ObjectiveSelection>(objectiveToSelectionDto);

        public static Objective ToModel(this SubobjectiveDto objectiveToListDto)
          => DtoMapper.Instance.Map<Objective>(objectiveToListDto);

        public static Objective ToModel(this ObjectiveToLocationDto objectiveToLocationDto)
            => DtoMapper.Instance.Map<Objective>(objectiveToLocationDto);

        public static Project ToModel(this ProjectToListDto projectToListDto)
            => DtoMapper.Instance.Map<Project>(projectToListDto);

        public static PagedData ToModel(this PagedDataDto pagedDataDto)
            => DtoMapper.Instance.Map<PagedData>(pagedDataDto);

        public static BimElementStatus ToModel(this BimElementStatusDto bimElementStatus)
          => DtoMapper.Instance.Map<BimElementStatus>(bimElementStatus);

        #endregion

        #region GetId

        public static ID<ProjectDto> GetId(this Project project)
           => DtoMapper.Instance.Map<ID<ProjectDto>>(project.ID);

        public static ID<ObjectiveDto> GetId(this Objective objective)
            => DtoMapper.Instance.Map<ID<ObjectiveDto>>(objective.ID);

        #endregion
    }
}
