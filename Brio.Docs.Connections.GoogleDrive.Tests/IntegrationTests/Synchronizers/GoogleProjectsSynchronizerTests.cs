using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.External.CloudBase.Synchronizers;
using Brio.Docs.Integration.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connections.GoogleDrive.Tests.IntegrationTests.Synchronizers
{
    [TestClass]
    public class GoogleProjectsSynchronizerTests
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
                    Name = "Google Drive",
                    AuthFieldNames = new List<string>() { "token" },
                    AppProperties = new Dictionary<string, string>
                    {
                        { "APPLICATION_NAME", "BRIO MRS" },
                        { "CLIENT_ID", "1827523568-ha5m7ddtvckjqfrmvkpbhdsl478rdkfm.apps.googleusercontent.com" },
                        { "CLIENT_SECRET", "fA-2MtecetmXLuGKXROXrCzt" },
                    },
                },
            };

            var connection = new GoogleConnection();
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

        [TestMethod]
        public async Task GetUpdatedIDs_AtLeastOneProjectAdded_RetrivedSuccessful()
        {
            var creationTime = DateTime.Now;
            var project = new ProjectExternalDto
            {
                Title = "TestProject123",
                UpdatedAt = creationTime,
            };
            var added = await synchronizer.Add(project);
            if (added?.ExternalID == null)
                Assert.Fail();
            await Task.Delay(1000);

            var result = await synchronizer.GetUpdatedIDs(creationTime.AddDays(-2));

            Assert.IsTrue(result.Any(o => o == added.ExternalID));
        }
    }
}
