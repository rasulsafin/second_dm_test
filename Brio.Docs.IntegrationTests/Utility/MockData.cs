using System;
using System.Collections.Generic;
using System.Globalization;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Common;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database.Models;

namespace Brio.Docs.Tests.Utility
{
    /// <summary>
    /// TODO: Test enums.
    /// </summary>
    public static class MockData
    {
        #region USERS

        public static List<User> DEFAULT_USERS
            => new List<User>
            {
                new User
                {
                    Login = PAULI_USER.Login,
                    Name = PAULI_USER.Name,
                    PasswordHash = PAULI_USER.PasswordHash,
                    PasswordSalt = PAULI_USER.PasswordSalt,
                    ExternalID = $"external id for {PAULI_USER.Login}",
                },
                new User
                {
                    Login = SCHREDINGER_USER.Login,
                    Name = SCHREDINGER_USER.Name,
                    PasswordHash = SCHREDINGER_USER.PasswordHash,
                    PasswordSalt = SCHREDINGER_USER.PasswordSalt,
                    ExternalID = $"external id for {SCHREDINGER_USER.Login}",
                },
                new User
                {
                    Login = HEISENBERG_USER.Login,
                    Name = HEISENBERG_USER.Name,
                    PasswordHash = HEISENBERG_USER.PasswordHash,
                    PasswordSalt = HEISENBERG_USER.PasswordSalt,
                    ExternalID = $"external id for {HEISENBERG_USER.Login}",
                },
                new User
                {
                    Login = BOHR_USER.Login,
                    Name = BOHR_USER.Name,
                    PasswordHash = BOHR_USER.PasswordHash,
                    PasswordSalt = BOHR_USER.PasswordSalt,
                    ExternalID = $"external id for {BOHR_USER.Login}",
                },
            };

        private static readonly User BOHR_USER = new User
        {
            Login = "NBohr",
            Name = "Nils Bohr",
            PasswordHash = new byte[10],
            PasswordSalt = new byte[5],
        };

        private static readonly User HEISENBERG_USER = new User
        {
            Login = "IAmTheDangerous",
            Name = "Werner Heisenberg",
            PasswordHash = new byte[10],
            PasswordSalt = new byte[5],
        };

        private static readonly User SCHREDINGER_USER = new User
        {
            Login = "loveDogs1932",
            Name = "Ervin Schredinger",
            PasswordHash = new byte[10],
            PasswordSalt = new byte[5],
        };

        private static readonly User PAULI_USER = new User
        {
            Login = "principlesHater",
            Name = "Wolfgang Pauli",
            PasswordHash = new byte[10],
            PasswordSalt = new byte[5],
        };

        public static User AdminUser => new User()
        {
            Login = "vpupkin",
            Name = "Vasily Pupkin",
            PasswordHash = new byte[] { 1, 2, 3, 4 },
            PasswordSalt = new byte[] { 5, 6, 7, 8 },
        };

        public static User OperatorUser => new User()
        {
            Login = "itaranov",
            Name = "Ivan Taranov",
            PasswordHash = new byte[] { 4, 8, 15, 16 },
            PasswordSalt = new byte[] { 23, 42, 6, 6 },
        };
        #endregion

        #region ROLES
        public static List<Role> DEFAULT_ROLES => new List<Role>
        {
            new Role { Name = ADMIN_ROLE.Name },
            new Role { Name = USER_ROLE.Name },
        };

        private static readonly Role ADMIN_ROLE = new Role { Name = "admin" };
        private static readonly Role USER_ROLE = new Role { Name = "user" };
        #endregion

        #region OBJECTIVE_TYPES
        public static List<ObjectiveType> DEFAULT_OBJECTIVE_TYPES => new List<ObjectiveType>
        {
            new ObjectiveType { Name = OBJECTIVE_TYPE_ONE.Name },
            new ObjectiveType { Name = OBJECTIVE_TYPE_TWO.Name },
        };

        private static readonly ObjectiveType OBJECTIVE_TYPE_ONE = new ObjectiveType { Name = "FirstOT" };
        private static readonly ObjectiveType OBJECTIVE_TYPE_TWO = new ObjectiveType { Name = "SecondOT" };
        #endregion

        #region CONNECTION_TYPES
        public static List<ConnectionType> DEFAULT_CONNECTION_TYPES => new List<ConnectionType>
        {
            new ConnectionType { Name = CONNECTION_TYPE_ONE.Name },
            new ConnectionType { Name = CONNECTION_TYPE_TWO.Name },
        };

        private static readonly ConnectionType CONNECTION_TYPE_ONE = new ConnectionType { Name = "FirstConnectionType" };
        private static readonly ConnectionType CONNECTION_TYPE_TWO = new ConnectionType { Name = "SecondConnectionType" };
        #endregion

        #region PROJECTS
        public static List<Project> DEFAULT_PROJECTS => new List<Project>
        {
            new Project { Title = GLADILOV_STREET.Title },
            new Project { Title = FSK.Title },
        };

        private static readonly Project GLADILOV_STREET = new Project { Title = "Gladilov str. 38a" };
        private static readonly Project FSK = new Project { Title = "FSK" };
        #endregion

        #region OBJECTIVES_TO_CREATE
        public static List<ObjectiveToCreateDto> DEFAULT_OBJECTIVES_TO_CREATE => new List<ObjectiveToCreateDto>
        {
            new ObjectiveToCreateDto
            {
                CreationDate = FIRST_TYPE_OPEN_OBJECTIVE_TO_CREATE.CreationDate,
                DueDate = FIRST_TYPE_OPEN_OBJECTIVE_TO_CREATE.DueDate,
                Title = FIRST_TYPE_OPEN_OBJECTIVE_TO_CREATE.Title,
                Description = FIRST_TYPE_OPEN_OBJECTIVE_TO_CREATE.Description,
                Status = FIRST_TYPE_OPEN_OBJECTIVE_TO_CREATE.Status,
            },
            new ObjectiveToCreateDto
            {
                CreationDate = FIRST_TYPE_INPROGRESS_OBJECTIVE_TO_CREATE.CreationDate,
                DueDate = FIRST_TYPE_INPROGRESS_OBJECTIVE_TO_CREATE.DueDate,
                Title = FIRST_TYPE_INPROGRESS_OBJECTIVE_TO_CREATE.Title,
                Description = FIRST_TYPE_INPROGRESS_OBJECTIVE_TO_CREATE.Description,
                Status = FIRST_TYPE_INPROGRESS_OBJECTIVE_TO_CREATE.Status,
            },
            new ObjectiveToCreateDto
            {
                CreationDate = SECOND_TYPE_INPROGRESS_OBJECTIVE_TO_CREATE.CreationDate,
                DueDate = SECOND_TYPE_INPROGRESS_OBJECTIVE_TO_CREATE.DueDate,
                Title = SECOND_TYPE_INPROGRESS_OBJECTIVE_TO_CREATE.Title,
                Description = SECOND_TYPE_INPROGRESS_OBJECTIVE_TO_CREATE.Description,
                Status = SECOND_TYPE_INPROGRESS_OBJECTIVE_TO_CREATE.Status,
            },
        };

        private static readonly ObjectiveToCreateDto FIRST_TYPE_OPEN_OBJECTIVE_TO_CREATE = new ObjectiveToCreateDto
        {
            CreationDate = DateTime.Now,
            DueDate = DateTime.MaxValue,
            Title = "First type OPEN issue",
            Description = "everything wrong! redo!!!",
            Status = ObjectiveStatus.Open,
        };

        private static readonly ObjectiveToCreateDto FIRST_TYPE_INPROGRESS_OBJECTIVE_TO_CREATE = new ObjectiveToCreateDto
        {
            CreationDate = DateTime.Now,
            DueDate = DateTime.MaxValue,
            Title = "First type OPEN issue",
            Description = "ASAP: everything wrong! redo!!!",
            Status = ObjectiveStatus.InProgress,
        };

        private static readonly ObjectiveToCreateDto SECOND_TYPE_INPROGRESS_OBJECTIVE_TO_CREATE = new ObjectiveToCreateDto
        {
            CreationDate = DateTime.Now,
            DueDate = DateTime.MaxValue,
            Title = "Second type OPEN issue",
            Description = "ASAP: everything wrong! redo!!!",
            Status = ObjectiveStatus.InProgress,
        };
        #endregion

        #region OBJECTIVES
        public static List<Objective> DEFAULT_OBJECTIVES => new List<Objective>
        {
            new Objective
            {
                CreationDate = FIRST_TYPE_OPEN_OBJECTIVE.CreationDate,
                DueDate = FIRST_TYPE_OPEN_OBJECTIVE.DueDate,
                Title = FIRST_TYPE_OPEN_OBJECTIVE.Title,
                Description = FIRST_TYPE_OPEN_OBJECTIVE.Description,
                Status = FIRST_TYPE_OPEN_OBJECTIVE.Status,
            },
            new Objective
            {
                CreationDate = FIRST_TYPE_INPROGRESS_OBJECTIVE.CreationDate,
                DueDate = FIRST_TYPE_INPROGRESS_OBJECTIVE.DueDate,
                Title = FIRST_TYPE_INPROGRESS_OBJECTIVE.Title,
                Description = FIRST_TYPE_INPROGRESS_OBJECTIVE.Description,
                Status = FIRST_TYPE_INPROGRESS_OBJECTIVE.Status,
            },
            new Objective
            {
                CreationDate = SECOND_TYPE_INPROGRESS_OBJECTIVE.CreationDate,
                DueDate = SECOND_TYPE_INPROGRESS_OBJECTIVE.DueDate,
                Title = SECOND_TYPE_INPROGRESS_OBJECTIVE.Title,
                Description = SECOND_TYPE_INPROGRESS_OBJECTIVE.Description,
                Status = SECOND_TYPE_INPROGRESS_OBJECTIVE.Status,
            },
        };

        private static readonly Objective FIRST_TYPE_OPEN_OBJECTIVE = new Objective
        {
            CreationDate = DateTime.Now,
            DueDate = DateTime.MaxValue,
            Title = "First type OPEN issue",
            Description = "everything wrong! redo!!!",
            Status = (int)ObjectiveStatus.Open,
        };

        private static readonly Objective FIRST_TYPE_INPROGRESS_OBJECTIVE = new Objective
        {
            CreationDate = DateTime.Now,
            DueDate = DateTime.MaxValue,
            Title = "First type OPEN issue",
            Description = "ASAP: everything wrong! redo!!!",
            Status = (int)ObjectiveStatus.InProgress,
        };

        private static readonly Objective SECOND_TYPE_INPROGRESS_OBJECTIVE = new Objective
        {
            CreationDate = DateTime.Now,
            DueDate = DateTime.MaxValue,
            Title = "Second type OPEN issue",
            Description = "ASAP: everything wrong! redo!!!",
            Status = (int)ObjectiveStatus.InProgress,
        };
        #endregion

        #region ITEMS

        public static List<Item> DEFAULT_ITEMS
            => new List<Item>
            {
                new Item
                {
                    ExternalID = FILE_ITEM.ExternalID,
                    ItemType = FILE_ITEM.ItemType,
                    RelativePath = FILE_ITEM.RelativePath,
                    Name = FILE_ITEM.Name,
                },
                new Item
                {
                    ExternalID = BIM_ITEM.ExternalID,
                    ItemType = BIM_ITEM.ItemType,
                    RelativePath = BIM_ITEM.RelativePath,
                    Name = BIM_ITEM.Name,
                },
                new Item
                {
                    ExternalID = MEDIA_ITEM.ExternalID,
                    ItemType = MEDIA_ITEM.ItemType,
                    RelativePath = MEDIA_ITEM.RelativePath,
                    Name = MEDIA_ITEM.Name,
                },
            };

        private static readonly Item FILE_ITEM = new Item
        {
            ExternalID = $"ExternalItemId{Guid.NewGuid()}",
            ItemType = 0,
            RelativePath = "File element",
            Name = "File element",
        };

        private static readonly Item BIM_ITEM = new Item
        {
            ExternalID = $"ExternalItemId{Guid.NewGuid()}",
            ItemType = 1,
            RelativePath = "Bim element",
            Name = "Bim element",
        };

        private static readonly Item MEDIA_ITEM = new Item
        {
            ExternalID = $"ExternalItemId{Guid.NewGuid()}",
            ItemType = 2,
            RelativePath = "Media element",
            Name = "Media element",
        };
        #endregion

        #region BIM_ELEMENTS
        public static List<BimElement> DEFAULT_BIM_ELEMENTS => new List<BimElement>
        {
            new BimElement { GlobalID = BIM_ELEMENT_ONE.GlobalID },
            new BimElement { GlobalID = BIM_ELEMENT_TWO.GlobalID },
        };

        private static readonly BimElement BIM_ELEMENT_ONE = new BimElement { GlobalID = $"GlobalId{Guid.NewGuid()}", ElementName = "Wall", ParentName = "House1" };
        private static readonly BimElement BIM_ELEMENT_TWO = new BimElement { GlobalID = $"GlobalId{Guid.NewGuid()}", ElementName = "Window", ParentName = "House2" };
        #endregion

        #region DYNAMIC_FIELDS_INFOS
        public static List<DynamicFieldInfo> DEFAULT_DYNAMIC_FIELD_INFOS => new List<DynamicFieldInfo>
        {
            new DynamicFieldInfo()
            {
                Name = "InfoField1",
                Value = "Value",
                Type = DynamicFieldType.STRING.ToString(),
            },
            new DynamicFieldInfo()
            {
                Name = "InfoField2",
                Value = "0",
                Type = DynamicFieldType.BOOL.ToString(),
            },
        };
        #endregion

        #region DYNAMIC_FIELDS
        public static List<DynamicField> DEFAULT_DYNAMIC_FIELDS => new List<DynamicField>
        {
            new DynamicField
            {
                Name = DYNAMIC_FIELD_STRING.Name,
                ExternalID = $"ExternalDFId{Guid.NewGuid()}-1",
                Type = DYNAMIC_FIELD_STRING.Type,
                Value = DYNAMIC_FIELD_STRING.Value,
                ChildrenDynamicFields = new List<DynamicField>(),
            },
            new DynamicField
            {
                Name = DYNAMIC_FIELD_DATE.Name,
                ExternalID = $"ExternalDFId{Guid.NewGuid()}-2",
                Type = DYNAMIC_FIELD_DATE.Type,
                Value = DYNAMIC_FIELD_DATE.Value,
                ChildrenDynamicFields = new List<DynamicField>(),
            },
        };

        private static readonly DynamicField DYNAMIC_FIELD_STRING =
            new DynamicField { Name = "string", Type = DynamicFieldType.STRING.ToString(), Value = "value" };

        private static readonly DynamicField DYNAMIC_FIELD_DATE =
            new DynamicField { Name = "datetime", Type = DynamicFieldType.DATE.ToString(), Value = DateTime.Now.ToString(CultureInfo.InvariantCulture) };

        #endregion

        public static ConnectionInfo TDMSConnectionInfo => new ConnectionInfo()
        {
            AuthFieldValues = new List<AuthFieldValue>() { new AuthFieldValue() { Key = "Key1", Value = "TDMS Value1" } },
        };

        public static ConnectionInfo BimConnectionInfo => new ConnectionInfo()
        {
            AuthFieldValues = new List<AuthFieldValue>() { new AuthFieldValue() { Key = "Key2", Value = "Bim Value2" } },
        };

        public static IEnumerable<EnumerationType> DEFAULT_ENUM_TYPES
            => new List<EnumerationType>()
            {
                new EnumerationType()
                {
                    Name = "1 type",
                    EnumerationValues = new List<EnumerationValue>()
                    {
                        new EnumerationValue()
                        {
                            Value = "1",
                            ExternalId = "1",
                        },
                        new EnumerationValue()
                        {
                            Value = "2",
                            ExternalId = "2",
                        },
                    },
                },
            };

        public static IEnumerable<EnumerationType> CreateEnumDms(string prefix, int count = 3)
        {
            for (int i = 0; i < count; i++)
                yield return new EnumerationType() { Name = $"{prefix} EnumDm {i + 1}" };
        }

        public static IEnumerable<EnumerationValue> CreateEnumDmValues(int enumDmID, string prefix, int count = 3)
        {
            for (int i = 0; i < count; i++)
                yield return new EnumerationValue() { Value = $"{prefix} Value {i + 1}", EnumerationTypeID = enumDmID };
        }
    }
}
