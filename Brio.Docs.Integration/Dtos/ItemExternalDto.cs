using System;
using System.IO;
using System.Runtime.Serialization;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.Integration.Dtos
{
    [DataContract]
    public class ItemExternalDto
    {
        [DataMember]
        public string ExternalID { get; set; }

        public string FileName => Path.GetFileName(RelativePath);

        public string FullPath => Path.Combine(ProjectDirectory, RelativePath);

        [DataMember]
        public ItemType ItemType { get; set; }

        [DataMember]
        public string ProjectDirectory { get; set; }

        [DataMember]
        public string RelativePath { get; set; }

        [DataMember]
        public DateTime UpdatedAt { get; set; }
    }
}
