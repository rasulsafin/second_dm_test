using System.Threading.Tasks;
using Brio.Docs.External.CloudBase.Synchronizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connections.GoogleDrive.Tests.ProjectInitializer
{
    /// <summary>
    /// Uncomment and run this tests to initialize standard project to test at the remote DM.
    /// </summary>
    [TestClass]
    public class GoogleDriveProjectInitializer
    {
        private static StorageProjectSynchronizer synchronizer;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext unused)
        {
            //var connectionInfo = new ConnectionInfoExternalDto
            //{
            //    ConnectionType = new ConnectionTypeExternalDto
            //    {
            //        Name = "Google Drive",
            //        AuthFieldNames = new List<string>() { "token" },
            //        AppProperties = new Dictionary<string, string>
            //        {
            //            { "APPLICATION_NAME", "BRIO MRS" },
            //            { "CLIENT_ID", "1827523568-ha5m7ddtvckjqfrmvkpbhdsl478rdkfm.apps.googleusercontent.com" },
            //            { "CLIENT_SECRET", "fA-2MtecetmXLuGKXROXrCzt" },
            //        },
            //    },
            //};

            //var connection = new GoogleConnection();
            //var context = await connection.GetContext(connectionInfo);
            //synchronizer = (StorageProjectSynchronizer)context.ProjectsSynchronizer;
        }

        [TestMethod]
        public async Task Add_NewProjectWithItems_AddedSuccessfully()
        {
            //// Change paths to ifc files paths
            //var item1 = new ItemExternalDto
            //{
            //    FileName = Path.GetFileName("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_AC_(IFC2x3)_05062020.ifczip"),
            //    FullPath = Path.GetFullPath("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_AC_(IFC2x3)_05062020.ifczip"),
            //    ItemType = ItemType.Bim,
            //};
            //var item2 = new ItemExternalDto
            //{
            //    FileName = Path.GetFileName("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_EOM_(IFC2x3)_25052020.ifczip"),
            //    FullPath = Path.GetFullPath("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_EOM_(IFC2x3)_25052020.ifczip"),
            //    ItemType = ItemType.Bim,
            //};
            //var item3 = new ItemExternalDto
            //{
            //    FileName = Path.GetFileName("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_OV_(IFC2x3)_06022020.ifczip"),
            //    FullPath = Path.GetFullPath("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_OV_(IFC2x3)_06022020.ifczip"),
            //    ItemType = ItemType.Bim,
            //};
            //var item4 = new ItemExternalDto
            //{
            //    FileName = Path.GetFileName("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_VK_(IFC2x3).ifczip"),
            //    FullPath = Path.GetFullPath("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_VK_(IFC2x3).ifczip"),
            //    ItemType = ItemType.Bim,
            //};
            //var item5 = new ItemExternalDto
            //{
            //    FileName = Path.GetFileName("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_VK_T3_(IFC2x3)_25052020.ifczip"),
            //    FullPath = Path.GetFullPath("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_VK_T3_(IFC2x3)_25052020.ifczip"),
            //    ItemType = ItemType.Bim,
            //};
            //var item6 = new ItemExternalDto
            //{
            //    FileName = Path.GetFileName("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_VK_В1_(IFC2x3)_25052020.ifczip"),
            //    FullPath = Path.GetFullPath("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_VK_В1_(IFC2x3)_25052020.ifczip"),
            //    ItemType = ItemType.Bim,
            //};
            //var item7 = new ItemExternalDto
            //{
            //    FileName = Path.GetFileName("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_VK_К1_(IFC4)_25052020.ifczip"),
            //    FullPath = Path.GetFullPath("C:\\Users\\diismagilov\\Downloads\\Telegram Desktop\\Гладилова\\00_Gladilova_VK_К1_(IFC4)_25052020.ifczip"),
            //    ItemType = ItemType.Bim,
            //};

            //var project = new ProjectExternalDto
            //{
            //    Title = "Гладилова 38а [Google Drive]",
            //    UpdatedAt = DateTime.Now,
            //    Items = new List<ItemExternalDto> { item1, item2, item3, item4, item5, item6, item7 },
            //};

            //var result = await synchronizer.Add(project);

            //Assert.IsNotNull(result?.ExternalID);
            //Assert.IsFalse(result.Items.Any(i => string.IsNullOrWhiteSpace(i.ExternalID)));
        }
    }
}
