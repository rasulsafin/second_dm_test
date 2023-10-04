using System;
using System.Collections.Generic;

namespace Brio.Docs.Reports.Models
{
    public class AttachedImageDetails
    {
        private static readonly HashSet<string> KnownSuffixes = new HashSet<string>() { "AR", "MR", "AMR", "VR", "RW", "CAM", "MAP" };

        private string imagePath;

        public string ImagePath
        {
            get => imagePath;
            set
            {
                if (imagePath != value)
                {
                    imagePath = value;
                    Suffix = GetSuffix(value);
                }
            }
        }

        public string Suffix { get; private set; } = string.Empty;

        public bool IsAR => string.Equals(Suffix, "AR", StringComparison.OrdinalIgnoreCase);

        public bool IsMR => string.Equals(Suffix, "MR", StringComparison.OrdinalIgnoreCase);

        public bool IsAMR => string.Equals(Suffix, "AMR", StringComparison.OrdinalIgnoreCase);

        public bool IsVR => string.Equals(Suffix, "VR", StringComparison.OrdinalIgnoreCase);

        public bool IsMap => string.Equals(Suffix, "MAP", StringComparison.OrdinalIgnoreCase);

        public bool IsRaw =>
            string.Equals(Suffix, "RW", StringComparison.OrdinalIgnoreCase)
            || string.Equals(Suffix, "CAM", StringComparison.OrdinalIgnoreCase);

        public bool IsUnknownMode => string.IsNullOrEmpty(Suffix) || !KnownSuffixes.Contains(Suffix.ToUpperInvariant());

        private static string GetSuffix(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return string.Empty;

            var fileName = System.IO.Path.GetFileNameWithoutExtension(imagePath);
            var suffix = System.IO.Path.GetExtension(fileName).TrimStart('.').ToUpperInvariant();
            return suffix;
        }
    }
}
