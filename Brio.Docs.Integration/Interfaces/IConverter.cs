using System.Threading.Tasks;

namespace Brio.Docs.Integration.Interfaces
{
    /// <summary>
    /// Represents a class that converts an object from one type to another type.
    /// </summary>
    /// <typeparam name="TFrom">The type of object that is to be converted.</typeparam>
    /// <typeparam name="TTo">The type the input object is to be converted to.</typeparam>
    public interface IConverter<in TFrom, TTo>
    {
        /// <summary>
        /// Converts an object from one type to another type.
        /// </summary>
        /// <param name="from">The object to convert.</param>
        /// <returns>The task of the operation.</returns>
        Task<TTo> Convert(TFrom from);
    }
}
