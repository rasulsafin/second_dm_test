using System;
using Brio.Docs.Client.Exceptions;
using Newtonsoft.Json;

namespace Brio.Docs.Client
{
    public class RequestResult
    {
        public RequestResult(object value)
            : this(value, null)
        {
        }

        public RequestResult(DocumentManagementException exception)
            : this(null, exception)
        {
        }

        [JsonConstructor]
        public RequestResult(object value, DocumentManagementException exception)
        {
            Value = value;
            Exception = exception;
        }

        public object Value { get; private set; }

        public DocumentManagementException Exception { get; set; }
    }
}
