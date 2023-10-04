using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.External.CloudBase.Synchronizers;
using Brio.Docs.Integration.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connections.YandexDisk.Tests.IntegrationTests.Synchronizers
{
    [TestClass]
    public class YandexObjectivesSynchronizerTests
    {
        private static readonly string TEST_FILE_PATH = "Resources/IntegrationTestFile.txt";
        private static StorageObjectiveSynchronizer synchronizer;

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
            synchronizer = (StorageObjectiveSynchronizer)context.ObjectivesSynchronizer;
        }

        [TestMethod]
        public async Task Add_ObjectiveWithEmptyId_AddedSuccessfully()
        {
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = "40179" },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
            };

            var result = await synchronizer.Add(objective);

            Assert.IsNotNull(result?.ExternalID);
        }

        [TestMethod]
        public async Task Add_ObjectiveWithEmptyIdWithItems_AddedSuccessfully()
        {
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = "40179" },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
                Items = new List<ItemExternalDto>
                {
                    new ItemExternalDto
                    {
                        RelativePath = TEST_FILE_PATH,
                        ProjectDirectory = Path.GetDirectoryName(Path.GetFullPath(TEST_FILE_PATH)),
                    },
                },
            };

            var result = await synchronizer.Add(objective);

            Assert.IsNotNull(result?.ExternalID);
            Assert.IsFalse(result.Items.Any(i => string.IsNullOrWhiteSpace(i.ExternalID)));
        }

        [TestMethod]
        public async Task Remove_JustAddedObjective_RemovedSuccessfully()
        {
            // Add
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = "40179" },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
            };
            var added = await synchronizer.Add(objective);
            if (added?.ExternalID == null)
                Assert.Fail("Objective adding failed. There is nothing to remove.");

            // Remove
            var result = await synchronizer.Remove(added);

            Assert.IsNotNull(result);
            Assert.AreEqual(ObjectiveStatus.Closed, result.Status);
        }

        [TestMethod]
        public async Task Update_JustAddedObjective_UpdatedSuccessfully()
        {
            var title = "First type OPEN issue";

            // Add
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = "40179" },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = title,
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
            };
            var added = await synchronizer.Add(objective);
            if (added?.ExternalID == null)
                Assert.Fail("Objective adding failed. There is nothing to update.");

            // Update
            var newTitle = added.Title = $"UPDATED: {title}";
            var result = await synchronizer.Update(added);

            Assert.IsNotNull(result?.Title);
            Assert.AreEqual(newTitle, result.Title);
        }

        [TestMethod]
        public async Task GetUpdatedIDs_AtLeastOneObjectiveAdded_RetrivedSuccessful()
        {
            var creationTime = DateTime.Now;
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = "40179" },
                CreationDate = creationTime,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
                UpdatedAt = creationTime,
            };
            var added = await synchronizer.Add(objective);
            if (added?.ExternalID == null)
                Assert.Fail();
            await Task.Delay(1000);

            var result = await synchronizer.GetUpdatedIDs(creationTime);

            Assert.IsNotNull(result.FirstOrDefault(o => o == added.ExternalID));
        }
    }
}
