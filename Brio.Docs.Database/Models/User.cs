using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Brio.Docs.Database.Models
{
    public class User
    {
        public int ID { get; set; }

        public string ExternalID { get; set; }

        [Required]
        [MinLength(1)]
        public string Login { get; set; }

        [Required]
        [MinLength(1)]
        public string Name { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }

        public int? ConnectionInfoID { get; set; }

        public ConnectionInfo ConnectionInfo { get; set; }

        public ICollection<Objective> Objectives { get; set; }

        public ICollection<UserProject> Projects { get; set; }

        public ICollection<UserRole> Roles { get; set; }

        public ICollection<Synchronization> Synchronizations { get; set; }
    }
}
