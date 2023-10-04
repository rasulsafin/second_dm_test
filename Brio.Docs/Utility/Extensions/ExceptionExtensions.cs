using System;
using System.Collections.Generic;
using Brio.Docs.Client.Exceptions;

namespace Brio.Docs.Utility.Extensions
{
    public static class ExceptionExtensions
    {
        public static DocumentManagementException ConvertToDocumentManagementException(this Exception exception)
        {
            if (exception == null)
                return null;

            var title = exception.Message;
            var details = exception.StackTrace;
            var errors = new Dictionary<string, string[]>();
            var inner = exception.InnerException;

            for (int i = 0; inner != null; i++)
            {
                errors.Add($"inner{i}", new[] { inner.Message, inner.StackTrace });
                inner = inner.InnerException;
            }

            return new DocumentManagementException(title, details, errors);
        }
    }
}
