using System;
using System.Collections.Generic;

namespace Brio.Docs.External
{
    public static class PathManager
    {
        /// <summary>
        /// "BRIO MRS".
        /// </summary>
        public static readonly string APPLICATION_ROOT_DIRECTORY_NAME = "BRIO MRS";

        /// <summary>
        /// "Tables".
        /// </summary>
        public static readonly string TABLE_DIRECTORY = "Tables";

        /// <summary>
        /// "Files".
        /// </summary>
        public static readonly string FILES_DIRECTORY = "Files";

        /// <summary>
        /// "Files/ProjectFiles".
        /// </summary>
        public static readonly string PROJECT_FILES_DIRECTORY = $"{FILES_DIRECTORY}/ProjectFiles";

        /// <summary>
        /// "{0}.json".
        /// </summary>
        public static readonly string RECORDED_FILE_FORMAT = "{0}.json";

        public static string GetLocalAppDir() => APPLICATION_ROOT_DIRECTORY_NAME;

        /// <summary>
        /// Returns "Files/ProjectFiles/&lt;<paramref name="projectFilesFolderName"/>>"
        /// </summary>
        /// <param name="projectFilesFolderName">The name of the project folder.</param>
        /// <returns>Files/ProjectFiles/&lt;<paramref name="projectFilesFolderName"/>></returns>
        public static string GetFilesDirectoryForProject(string projectFilesFolderName)
            => Join(PROJECT_FILES_DIRECTORY, projectFilesFolderName);

        public static string GetTablesDir() => DirectoryName(APPLICATION_ROOT_DIRECTORY_NAME, TABLE_DIRECTORY);

        public static string GetTableDir(string tableName) => DirectoryName(GetTablesDir(), tableName);

        public static string GetFile(string dirName, string fileName) => DirectoryName(GetNestedDirectory(dirName), fileName);

        public static string GetNestedDirectory(string dirName) => DirectoryName(APPLICATION_ROOT_DIRECTORY_NAME, dirName);

        public static string GetRecordFile(string tableName, string id) => FileName(GetTableDir(tableName), string.Format(RECORDED_FILE_FORMAT, id));

        public static string GetRootDirectory() => DirectoryName("/", APPLICATION_ROOT_DIRECTORY_NAME);

        public static string DirectoryName(string path, string nameDir)
        {
            List<string> items = new List<string>(path.Split('/', StringSplitOptions.RemoveEmptyEntries));
            items.Add(nameDir);
            string result = string.Join('/', items);
            return $"/{result}/";
        }

        public static string FileName(string path, string nameFile)
        {
            List<string> items = new List<string>(path.Split('/', StringSplitOptions.RemoveEmptyEntries));
            items.Add(nameFile);
            string result = string.Join('/', items);
            return $"/{result}";
        }

        /// <summary>
        /// Concatenates two cloud paths into a single path.
        /// A slash is added between paths if the first path does not end with a slash and the second path does not start with a slash.
        /// </summary>
        /// <param name="path1">The first path to join.</param>
        /// <param name="path2">The second path to join.</param>
        /// <returns>The concatenated path.</returns>
        public static string Join(string path1, string path2)
        {
            var needDelimiter = !path1.EndsWith('/') && !path2.StartsWith('/');
            return needDelimiter ? $"{path1}/{path2}" : path1 + path2;
        }

        public static string ConvertToVirtualPath(string localPath)
            => localPath.Replace('\\', '/');
    }
}
