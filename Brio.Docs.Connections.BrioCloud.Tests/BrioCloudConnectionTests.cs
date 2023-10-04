using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.External.Extensions;
using Brio.Docs.Integration.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

namespace Brio.Docs.Connections.BrioCloud.Tests
{
    [TestClass]
    public class BrioCloudConnectionTests
    {
        private const string USERNAME = "Username";
        private const string PASSWORD = "Password";

        private const string VALID_USERNAME = "briomrs";
        private const string VALID_PASSWORD = "BrioMRS2021";

        private static readonly string NAME_CONNECTION = BrioCloudConnection.CONNECTION_NAME;

        private static ConnectionInfoExternalDto validInfo;

        [ClassInitialize]
        public static void ClassInitialize(TestContext unused)
        {
            validInfo = new ConnectionInfoExternalDto()
            {
                ConnectionType = new ConnectionTypeExternalDto()
                {
                    Name = NAME_CONNECTION,
                },
                AuthFieldValues = new Dictionary<string, string>
                {
                    { USERNAME, VALID_USERNAME },
                    { PASSWORD, VALID_PASSWORD },
                },
            };
        }

        [TestMethod]
        public async Task Connect_ValidCredentials_OK()
        {
            var connection = new BrioCloudConnection();

            var expectedResult = RemoteConnectionStatus.OK;
            var result = await connection.Connect(validInfo, default);

            Assert.AreEqual(expectedResult, result.Status);
        }

        [TestMethod]
        [DataRow("briomrs", "BrioMRS2020")]
        public async Task Connect_InvalidCredentials_Error(string username, string password)
        {
            var info = new ConnectionInfoExternalDto()
            {
                ConnectionType = new ConnectionTypeExternalDto()
                {
                    Name = NAME_CONNECTION,
                },
                AuthFieldValues = new Dictionary<string, string>
                {
                    { USERNAME, username },
                    { PASSWORD, password },
                },
            };

            var connection = new BrioCloudConnection();
            var expectedResult = RemoteConnectionStatus.NeedReconnect;

            var result = await connection.Connect(info, default);

            Assert.AreEqual(expectedResult, result.Status);
        }

        [TestMethod]
        public async Task Connect_WithoutCredentials_Error()
        {
            var info = new ConnectionInfoExternalDto()
            {
                ConnectionType = new ConnectionTypeExternalDto()
                {
                    Name = NAME_CONNECTION,
                },
            };

            var connection = new BrioCloudConnection();
            var expectedResult = RemoteConnectionStatus.Error;

            var result = await connection.Connect(info, default);

            Assert.AreEqual(expectedResult, result.Status);
        }

        [TestMethod]
        public async Task GetStatus_ManagerInitiated_OK()
        {
            var connection = new BrioCloudConnection();
            await connection.Connect(validInfo, default);

            var expectedResult = RemoteConnectionStatus.OK;
            var result = await connection.GetStatus(validInfo);

            Assert.AreEqual(expectedResult, result.Status);
        }

        [TestMethod]
        public async Task GetStatus_ManagerNotInitiated_NeedReconnect()
        {
            var connection = new BrioCloudConnection();

            var expectedResult = RemoteConnectionStatus.NeedReconnect;
            var result = await connection.GetStatus(validInfo);

            Assert.AreEqual(expectedResult, result.Status);
        }

        [TestMethod]
        public async Task UpdateConnectionInfo_NoExternalId_InfoWithExternalIdSameAsUsername()
        {
            var connection = new BrioCloudConnection();

            var connectionInfo = await connection.UpdateConnectionInfo(validInfo);

            Assert.IsFalse(string.IsNullOrWhiteSpace(connectionInfo.UserExternalID));
            Assert.AreEqual(validInfo.GetAuthValue(BrioCloudAuth.USERNAME), connectionInfo.UserExternalID);
        }

        [TestMethod]
        public async Task UpdateConnectionInfo_WrongObjectiveTypeWithoutGuid_NewObjectiveTypeNewGuid()
        {
            string objectiveType = "WrongObjectiveType";
            var connection = new BrioCloudConnection();
            var info = new ConnectionInfoExternalDto()
            {
                ConnectionType = new ConnectionTypeExternalDto()
                {
                    ObjectiveTypes = new List<ObjectiveTypeExternalDto>
                    {
                        new ObjectiveTypeExternalDto { Name = objectiveType, ExternalId = objectiveType },
                    },
                },
            };

            var expectedResult = "BrioCloudIssue";
            var connectionInfo = await connection.UpdateConnectionInfo(info);
            var result = connectionInfo.ConnectionType.ObjectiveTypes.FirstOrDefault();

            Assert.AreEqual(expectedResult, result.Name);
            Assert.AreEqual(expectedResult, result.ExternalId);
        }

        [TestMethod]
        public async Task UpdateConnectionInfo_WrongObjectiveTypeWithGuid_NewObjectiveTypeOldGuid()
        {
            string objectiveType = "WrongObjectiveType";
            var userExternalId = Guid.NewGuid().ToString();
            var connection = new BrioCloudConnection();
            var info = new ConnectionInfoExternalDto()
            {
                ConnectionType = new ConnectionTypeExternalDto()
                {
                    ObjectiveTypes = new List<ObjectiveTypeExternalDto>
                    {
                        new ObjectiveTypeExternalDto { Name = objectiveType, ExternalId = objectiveType },
                    },
                },
                UserExternalID = userExternalId,
            };

            var expectedResult = "BrioCloudIssue";
            var connectionInfo = await connection.UpdateConnectionInfo(info);

            Assert.AreEqual(userExternalId, connectionInfo.UserExternalID);

            var result = connectionInfo.ConnectionType.ObjectiveTypes.FirstOrDefault();

            Assert.AreEqual(expectedResult, result.Name);
            Assert.AreEqual(expectedResult, result.ExternalId);
        }

        [TestMethod]
        public async Task UpdateConnectionInfo_RightObjectiveTypeWithGuid_OldObjectiveTypeOldGuid()
        {
            string objectiveType = "BrioCloudIssue";
            var userExternalId = Guid.NewGuid().ToString();
            var connection = new BrioCloudConnection();
            var info = new ConnectionInfoExternalDto()
            {
                ConnectionType = new ConnectionTypeExternalDto()
                {
                    ObjectiveTypes = new List<ObjectiveTypeExternalDto>
                    {
                        new ObjectiveTypeExternalDto { Name = objectiveType, ExternalId = objectiveType },
                    },
                },
                UserExternalID = userExternalId,
            };

            var connectionInfo = await connection.UpdateConnectionInfo(info);

            Assert.AreEqual(userExternalId, connectionInfo.UserExternalID);

            var result = connectionInfo.ConnectionType.ObjectiveTypes.FirstOrDefault();

            Assert.AreEqual(objectiveType, result.Name);
            Assert.AreEqual(objectiveType, result.ExternalId);
        }

        [TestMethod]
        public async Task UpdateConnectionInfo_RightObjectiveTypeWithoutGuid_OldObjectiveTypeNewGuid()
        {
            string objectiveType = "BrioCloudIssue";
            var connection = new BrioCloudConnection();
            var info = new ConnectionInfoExternalDto()
            {
                ConnectionType = new ConnectionTypeExternalDto()
                {
                    ObjectiveTypes = new List<ObjectiveTypeExternalDto>
                    {
                        new ObjectiveTypeExternalDto { Name = objectiveType, ExternalId = objectiveType },
                    },
                },
            };
            await connection.Connect(info, default);

            var connectionInfo = await connection.UpdateConnectionInfo(info);
            var result = connectionInfo.ConnectionType.ObjectiveTypes.FirstOrDefault();

            Assert.AreEqual(objectiveType, result.Name);
            Assert.AreEqual(objectiveType, result.ExternalId);
        }

        [TestMethod]
        public async Task GetContext_ValidConnectionInfo_NotNull()
        {
            var connection = new BrioCloudConnection();
            var info = new ConnectionInfoExternalDto()
            {
                AuthFieldValues = new Dictionary<string, string>
                {
                    { USERNAME, VALID_USERNAME },
                    { PASSWORD, VALID_PASSWORD },
                },
            };

            var result = await connection.GetContext(info);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public async Task GetContext_InvalidConnectionInfo_NotNull()
        {
            var connection = new BrioCloudConnection();
            var info = new ConnectionInfoExternalDto()
            {
            };

            await connection.GetContext(info);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public async Task GetStorage_InvalidConnectionInfo_NotNull()
        {
            var connection = new BrioCloudConnection();
            var info = new ConnectionInfoExternalDto()
            {
            };

            await connection.GetStorage(info);
        }
    }
}
