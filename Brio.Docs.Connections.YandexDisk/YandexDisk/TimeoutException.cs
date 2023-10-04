using System;

namespace Brio.Docs.Integration
{
    public class TimeoutException : Exception
    {
        public TimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
