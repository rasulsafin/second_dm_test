namespace Brio.Docs.Client.Exceptions
{
    public class NotFoundException<T> : ANotFoundException
    {
        public NotFoundException(int id)
            : base($"{typeof(T).Name} with key {id} not found")
        {
        }

        public NotFoundException(string propertyName, string propertyValue)
            : base($"{typeof(T).Name} with {propertyName?.ToLower()} {propertyValue} not found")
        {
        }

        public NotFoundException(string message)
           : base(message)
        {
        }
    }
}
