using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.External.Utils;

namespace Brio.Docs.External
{
    /// <summary>
    /// Manager to handle work with cloud storages like DMs.
    /// </summary>
    public interface ICloudManager
    {
        /// <summary>
        /// Скачать объект с сервера из соответвующей таблицы. Если объекта нет вернётся null.
        /// </summary>
        /// <typeparam name="T"> Тип объекта и имя папки.</typeparam>
        /// <param name="id"> id объекта.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<T> Pull<T>(string id);

        /// <summary>
        /// Отправить объект на сервер в папку соответвующую названию Типа.
        /// </summary>
        /// <typeparam name="T">Тип объекта и имя папки.</typeparam>
        /// <param name="object"> конеченый объект. </param>
        /// <param name="id">id объекта.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> Push<T>(T @object, string id);

        /// <summary>
        /// Удалить файл.
        /// </summary>
        /// <param name="href">уникальный путь к файлу.</param>
        /// <returns>Delete result.</returns>
        Task<bool> DeleteFile(string href);

        /// <summary>
        /// Получить файл.
        /// </summary>
        /// <param name="href">уникальный путь к файлу.</param>
        /// <param name="fileName">имя файла.</param>
        /// <returns>Pull result.</returns>
        Task<bool> PullFile(string href, string fileName);

        /// <summary>
        /// Отправить файл.
        /// </summary>
        /// <param name="remoteDirName">Название подпапки в папке приложения 'BRIO MRS'.</param>
        /// <param name="fullPath">Full path to the file.</param>
        /// <returns>Push result.</returns>
        Task<string> PushFile(string remoteDirName, string fullPath);

        /// <summary>
        /// Удалить объект из папки.
        /// </summary>
        /// <typeparam name="T">Тип объекта и имя папки.</typeparam>
        /// <param name="id">id объекта.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<bool> Delete<T>(string id);

        /// <summary>
        /// Pulls all items from remote path.
        /// </summary>
        /// <typeparam name="T">Type of items.</typeparam>
        /// <param name="path">Remote folder path.</param>
        /// <returns>List of items.</returns>
        Task<List<T>> PullAll<T>(string path);

        /// <summary>
        /// Gets cloud elements from remote folder.
        /// </summary>
        /// <param name="directoryPath">Remote folder path.</param>
        /// <returns>Enumeration of cloud elements.</returns>
        Task<IEnumerable<CloudElement>> GetRemoteDirectoryFiles(string directoryPath = "/");
    }
}
