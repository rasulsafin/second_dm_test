using System;
using System.IO;
using System.Linq;
using Brio.Docs.Database.Models;

namespace Brio.Docs.Utility
{
    public class PathHelper
    {
        private static readonly string APPLICATION_DIRECTORY_NAME = "Brio MRS";
        private static readonly string DATABASE_DIRECTORY_NAME = "Database";
        private static readonly string MY_DOCUMENTS = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private static readonly char[] INVALID_PATH_CHARS = Path.GetInvalidFileNameChars().Append('.').ToArray();

        public static string ApplicationFolder => Combine(MY_DOCUMENTS, APPLICATION_DIRECTORY_NAME);

        public static string Database => Combine(ApplicationFolder, DATABASE_DIRECTORY_NAME);

        public static string GetDirectory(Project project)
            => Combine(Database, GetValidDirectoryName(project));

        public static string GetFileName(string path)
            => Path.GetFileName(path);

        public static string GetValidDirectoryName(Project project) =>
            // TODO : You can trim the project title, for example, by taking the first 50 characters.
            GetValidDirectoryName(project.Title);

        private static string GetValidDirectoryName(string name)
        {
            foreach (char c in INVALID_PATH_CHARS)
                name = name.Replace(c, '~');
            return name;
        }

        private static string Combine(params string[] strings)
            => Path.GetFullPath(Path.Combine(strings));
    }
}
