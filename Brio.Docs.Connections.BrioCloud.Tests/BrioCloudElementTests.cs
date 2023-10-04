using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebDav;

namespace Brio.Docs.Connections.BrioCloud.Tests
{
    [TestClass]
    public class BrioCloudElementTests
    {
        [TestMethod]
        public void GetElements_MixedElements_ValidCount()
        {
            var builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri1");
            builder.IsCollection();
            var folderElement1 = builder.Build();

            builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri2");
            builder.IsCollection();
            builder.WithContentType("SomeContentType2");
            var folderElement2 = builder.Build();

            var folders = new List<WebDavResource>
                {
                    folderElement1,
                    folderElement2,
                };

            builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri1");
            builder.WithContentType("SomeContentType1");
            builder.IsNotCollection();
            var fileElement1 = builder.Build();

            builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri2");
            builder.WithContentType("SomeContentType2");
            builder.IsNotCollection();
            var fileElement2 = builder.Build();

            builder = new WebDavResource.Builder();
            builder.WithUri("SomeUri3");
            builder.WithContentType("SomeContentType3");
            builder.IsNotCollection();
            var fileElement3 = builder.Build();

            var files = new List<WebDavResource>
                {
                    fileElement1,
                    fileElement2,
                    fileElement3,
                };

            var collection = new List<WebDavResource>();
            collection.AddRange(folders);
            collection.AddRange(files);

            var result = BrioCloudElement.GetElements(collection, string.Empty);

            int foldersCount = result.Where(r => r.IsDirectory).Count();
            int filesCount = result.Where(r => !r.IsDirectory).Count();

            Assert.AreEqual(folders.Count, foldersCount);
            Assert.AreEqual(files.Count, filesCount);
        }

        [TestMethod]

        // remove
        [DataRow("/remote.php/dav/files/username/1.txt", "/1.txt")]
        [DataRow("/remote.php/dav/files/username/remote.php/dav/files/username/1.txt", "/remote.php/dav/files/username/1.txt")]
        [DataRow("/remote.php/dav/files/username@example.com/1.txt", "/1.txt")]
        [DataRow("/remote.php/dav/files/username/folder/1.txt", "/folder/1.txt")]
        [DataRow("/remote.php/dav/files/username/folder1/folder2/1.txt", "/folder1/folder2/1.txt")]
        [DataRow("/remote.php/dav/files//1.txt", "/1.txt")]

        // do not remove
        [DataRow("/1.txt", "/1.txt")]
        [DataRow("/////1.txt", "/////1.txt")]
        [DataRow("/folder/1.txt", "/folder/1.txt")]
        [DataRow("/folder1/folder2/1.txt", "/folder1/folder2/1.txt")]
        [DataRow("/remote.php/dav//files/username/1.txt", "/remote.php/dav//files/username/1.txt")]
        [DataRow("/remote.php/dev/files/username/1.txt", "/remote.php/dev/files/username/1.txt")]
        [DataRow("/remote.php/dav/files/1.txt", "/remote.php/dav/files/1.txt")]
        [DataRow("/folder/remote.php/dav/files/username/1.txt", "/folder/remote.php/dav/files/username/1.txt")]
        public void GetElements_SourceContainsFile_ElementHrefDoesNotContainPhpMethodAndUserName(string fileUri, string expectingResult)
        {
            // Arrange
            var builder = new WebDavResource.Builder();
            builder.WithUri(Uri.EscapeDataString(fileUri));
            builder.IsNotCollection();
            var file = builder.Build();

            var source = new[] { file };
            var ignoringUri = string.Empty;

            // Act
            var result = BrioCloudElement.GetElements(source, ignoringUri);
            var href = result.First().Href;

            // Assert
            Assert.AreEqual(expectingResult, href, "Href differs from expected result");
        }
    }
}
