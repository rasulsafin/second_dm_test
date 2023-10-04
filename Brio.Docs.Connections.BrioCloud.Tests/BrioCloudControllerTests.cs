using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connections.BrioCloud.Tests
{
    [TestClass]
    public class BrioCloudControllerTests
    {
        private const string VALID_USERNAME = "briomrs";
        private const string VALID_PASSWORD = "BrioMRS2021";

        private const string ROOT_PATH = "/";
        private const string TEST_FOLDER_PATH = "/BRIO MRS/TESTS";
        private const string TEST_EXISTENT_FOLDER = "TestExistentfolder";
        private const string TEST_NOT_EXISTENT_FOLDER = "TestNotExistentfolder";
        private const string TEST_FOLDER_FOR_CREATE_NAME = "Testfolder";

        private static readonly string TEST_CONTENT_FILE_URI = $"{TEST_FOLDER_PATH}/ContentFile";
        private static readonly string TEST_CONTENT = "TestContent";

        private static BrioCloudController controller;
        private static string testFileUri;

        private static string TempFilePath
        {
            get
            {
                return Path.GetTempFileName();
            }
        }

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext unused)
        {
            controller = new BrioCloudController(VALID_USERNAME, VALID_PASSWORD);

            try
            {
                await controller.GetListAsync(TEST_FOLDER_PATH);
            }
            catch (FileNotFoundException)
            {
                await controller.CreateDirAsync("/", TEST_FOLDER_PATH);
            }

            testFileUri = await controller.UploadFileAsync(TEST_FOLDER_PATH, TempFilePath);
            await controller.SetContentAsync(TEST_CONTENT_FILE_URI, TEST_CONTENT);
            try
            {
                await controller.CreateDirAsync(TEST_FOLDER_PATH, TEST_EXISTENT_FOLDER);
            }
            catch
            {
            }

            try
            {
                await controller.DeleteAsync($"{TEST_FOLDER_PATH}/{TEST_FOLDER_FOR_CREATE_NAME}");
            }
            catch
            {
            }

            try
            {
                await controller.DeleteAsync($"{TEST_FOLDER_PATH}/{TEST_NOT_EXISTENT_FOLDER}");
            }
            catch
            {
            }
        }

        [TestMethod]
        [DataRow(ROOT_PATH)]
        [DataRow(TEST_FOLDER_PATH)]
        public async Task GetListAsync_ExistentPath_IsNotNull(string path)
        {
            var result = await controller.GetListAsync(path);

            Assert.IsTrue(result.Count() > 0);
        }

        [TestMethod]
        [DataRow("/NotExistentPath")]
        [ExpectedException(typeof(FileNotFoundException))]
        public async Task GetListAsync_NotExistentPath_FileNotFoundException(string path)
        {
            await controller.GetListAsync(path);
        }

        [TestMethod]
        public async Task DownloadFileAsync_ExistentFile_IsTrue()
        {
            var result = await controller.DownloadFileAsync(testFileUri, TempFilePath);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [DataRow("/NotExistentFile")]
        [ExpectedException(typeof(FileNotFoundException))]
        public async Task DownloadFileAsync_NotExistentFile_FileNotFoundException(string href)
        {
            await controller.DownloadFileAsync(href, TempFilePath);
        }

        [TestMethod]
        [DataRow(TEST_FOLDER_PATH)]
        public async Task UploadFileAsync_ExistentFile_IsTrue(string href)
        {
            var result = await controller.UploadFileAsync(href, TempFilePath);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [DataRow("/NotExistentFile")]
        [ExpectedException(typeof(WebException))]
        public async Task UploadFileAsync_NotExistentFile_WebException(string href)
        {
            try
            {
                await controller.DeleteAsync(href);
            }
            catch
            {
            }

            await controller.UploadFileAsync(href, TempFilePath);
        }

        [TestMethod]
        [DataRow(TEST_FOLDER_PATH, TEST_EXISTENT_FOLDER)]
        [ExpectedException(typeof(WebException))]
        public async Task CreateDirAsync_ValidPathExistentFolder_WebException(string path, string dir)
        {
            await controller.CreateDirAsync(path, dir);
        }

        [TestMethod]
        [DataRow(TEST_FOLDER_PATH, TEST_FOLDER_FOR_CREATE_NAME)]
        public async Task CreateDirAsync_ValidPathNotExistentFolder_IsNotNull(string path, string dir)
        {
            try
            {
                await controller.DeleteAsync($"{path}/{dir}");
            }
            catch
            {
            }

            var result = await controller.CreateDirAsync(path, dir);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DeleteAsync_ExistentFile_IsTrue()
        {
            var tempFile = await controller.UploadFileAsync(TEST_FOLDER_PATH, TempFilePath);

            var result = await controller.DeleteAsync(tempFile);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [DataRow("/NotExistentFile")]
        public async Task DeleteAsync_NotExistentFile_IsFalse(string href)
        {
            var result = await controller.DeleteAsync(href);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetContentAsync_ExistentFile_IsTrue()
        {
            var result = await controller.GetContentAsync(TEST_CONTENT_FILE_URI);

            Assert.AreEqual(result, TEST_CONTENT);
        }

        [TestMethod]
        [DataRow("/NotExistentFile")]
        [ExpectedException(typeof(FileNotFoundException))]
        public async Task GetContentAsync_NotExistentFile_IsFalse(string href)
        {
            await controller.GetContentAsync(href);
        }

        [TestMethod]
        public async Task SetContentAsync_ExistentFolder_IsTrue()
        {
            var result = await controller.SetContentAsync(TEST_CONTENT_FILE_URI, TEST_CONTENT);

            Assert.IsTrue(result, TEST_CONTENT);
        }

        [TestMethod]
        [DataRow(TEST_FOLDER_PATH + "/NotExistentFolder")]
        public async Task SetContentAsync_NotExistentFolder_IsTrue(string href)
        {
            var result = await controller.SetContentAsync(href, TEST_CONTENT);

            Assert.IsTrue(result);
        }
    }
}
