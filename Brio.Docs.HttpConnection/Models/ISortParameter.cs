namespace Brio.Docs.HttpConnection.Models
{
    /// <summary>
    /// Readonly sort definition.
    /// </summary>
    public interface ISortParameter
    {
        /// <summary>
        /// Object property name to be sorted by.
        /// </summary>
        string FieldName { get; }

        /// <summary>
        /// Sort direction.
        /// </summary>
        bool IsDescending { get; }
    }
}
