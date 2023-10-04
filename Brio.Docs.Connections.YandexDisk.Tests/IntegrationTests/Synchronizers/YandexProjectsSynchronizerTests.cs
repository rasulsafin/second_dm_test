using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.External.CloudBase.Synchronizers;
using Brio.Docs.Integration.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connections.YandexDisk.Tests.IntegrationTests.Synchronizers
{
    [TestClass]
    public class YandexProjectsSynchronizerTests
    {
        private static readonly string TEST_FILE_PATH = "Resources/IntegrationTestFile.txt";
        private static StorageProjectSynchronizer synchronizer;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext unused)
        {
            var connectionInfo = new ConnectionInfoExternalDto
            {
                ConnectionType = new ConnectionTypeExternalDto
                {
                    Name = "Yandex Disk",
                    AuthFieldNames = new List<string>() { "token" },
                    AppProperties = new Dictionary<string, string>
                    {
                        { "CLIENT_ID", "b1a5acbc911b4b31bc68673169f57051" },
                        { "CLIENT_SECRET", "b4890ed3aa4e4a4e9e207467cd4a0f2c" },
                        { "RETURN_URL", @"http://localhost:8000/oauth/" },
                    },
                },
            };

            var connection = new YandexConnection();
            var context = await connection.GetContext(connectionInfo);
            synchronizer = (StorageProjectSynchronizer)context.ProjectsSynchronizer;
        }

        [TestMethod]
        public async Task Add_NewProjectWithoutItems_AddedSuccessfully()
        {
            var project = new ProjectExternalDto
            {
                Title = "TestProject123",
                UpdatedAt = DateTime.Now,
            };

            var result = await synchronizer.Add(project);

            Assert.IsNotNull(result?.ExternalID);
        }

        [TestMethod]
        public async Task Add_NewProjectWithItems_AddedSuccessfully()
        {
            var project = new ProjectExternalDto
            {
                Title = "TestProject123",
                UpdatedAt = DateTime.Now,
                Items = new List<ItemExternalDto>
                {
                    new ItemExternalDto
                    {
                        RelativePath = TEST_FILE_PATH,
                        ProjectDirectory = Path.GetDirectoryName(Path.GetFullPath(TEST_FILE_PATH)),
                    },
                },
            };

            var result = await synchronizer.Add(project);

            Assert.IsNotNull(result?.ExternalID);
            Assert.IsTrue(result.Items.Any());
            Assert.IsFalse(result.Items.Any(i => string.IsNullOrWhiteSpace(i.ExternalID)));
        }

        [TestMethod]
        public async Task Remove_JustAddedProject_RemovedSuccessfully()
        {
            // Add
            var project = new ProjectExternalDto
            {
                Title = "TestProject123",
                UpdatedAt = DateTime.Now,
            };
            var added = await synchronizer.Add(project);
            if (added?.ExternalID == null)
                Assert.Fail("Project adding failed. There is nothing to remove.");

            // Remove
            await Task.Delay(1000);
            var result = await synchronizer.Remove(added);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Update_JustAddedProject_UpdatedSuccessfully()
        {
            var title = "First type OPEN issue";

            // Add
            var project = new ProjectExternalDto
            {
                Title = "TestProject123",
                UpdatedAt = DateTime.Now,
            };
            var added = await synchronizer.Add(project);
            if (added?.ExternalID == null)
                Assert.Fail("Project adding failed. There is nothing to update.");

            // Update
            var newTitle = added.Title = $"UPDATED: {title}";
            var result = await synchronizer.Update(added);

            Assert.IsNotNull(result?.Title);
            Assert.AreEqual(newTitle, result.Title);
        }
    }
}
