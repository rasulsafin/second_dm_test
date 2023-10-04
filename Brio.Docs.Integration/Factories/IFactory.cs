namespace Brio.Docs.Integration.Factories
{
    /// <summary>
    /// A factory for creating instances of T.
    /// </summary>
    /// <typeparam name="T">The factory result type.</typeparam>
    public interface IFactory<out T>
    {
        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <returns>The instance.</returns>
        T Create();
    }

    /// <summary>
    /// A factory for creating instances of T.
    /// </summary>
    /// <typeparam name="TParameter">The parameter needed for creating instance.</typeparam>
    /// <typeparam name="T">The factory result type.</typeparam>
    public interface IFactory<in TParameter, out T>
    {
        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="parameter">The parameter needed for creating instance.</param>
        /// <returns>The instance.</returns>
        T Create(TParameter parameter);
    }

    /// <summary>
    /// A factory for creating instances of T.
    /// </summary>
    /// <typeparam name="TParameter1">The parameter needed for creating instance.</typeparam>
    /// <typeparam name="TParameter2">The second parameter needed for creating instance.</typeparam>
    /// <typeparam name="T">The factory result type.</typeparam>
    public interface IFactory<in TParameter1, in TParameter2, out T>
    {
        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="parameter1">The parameter needed for creating instance.</param>
        /// <param name="parameter2">The second parameter needed for creating instance.</param>
        /// <returns>The instance.</returns>
        T Create(TParameter1 parameter1, TParameter2 parameter2);
    }
}
