using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.External
{
    public static class ItemTypeHelper
    {
        // TODO: improve this.
        private static readonly IReadOnlyCollection<string> BIM_EXTENSIONS = new[]
        {
            ".ifc",
            ".ifczip",
            ".ifcxml",
            ".nwc",
            ".nwd",
            ".nwf",
            ".fbx",
            ".stl",
            ".rvt",
            ".stp",
            ".step",
            ".dwg",
            ".obj",
        };

        private static readonly IReadOnlyCollection<string> MEDIA_EXTENSIONS = new[] { ".jpg", ".jpeg", ".mp4", ".png" };

        public static ItemType GetTypeByName(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return BIM_EXTENSIONS.Any(s => string.Equals(s, extension, StringComparison.InvariantCultureIgnoreCase))
                ? ItemType.Bim
                : MEDIA_EXTENSIONS.Any(s => string.Equals(s, extension, StringComparison.InvariantCultureIgnoreCase))
                    ? ItemType.Media
                    : ItemType.File;
        }
    }
}
