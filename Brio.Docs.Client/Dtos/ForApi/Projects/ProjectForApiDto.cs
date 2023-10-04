using System.Collections.Generic;

namespace Brio.Docs.Client.Dtos.ForApi.Project
{
    public class ProjectForApiDto : BaseForApiDto
    {
        public ProjectForApiDto()
        {
            // ID 1 corresponds to "Brio Mrs" in the API database.
            OrganizationId = 1;
            IsInArchive = false;
        }

        public string Title { get; set; }

        /// <summary>
        /// Organization : BrioMrs.
        /// </summary>
        public long OrganizationId { get; set;  }

        public bool IsInArchive { get; set; }
    }
}
