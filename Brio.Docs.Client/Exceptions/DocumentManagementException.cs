using System;
using System.Collections.Generic;

namespace Brio.Docs.Client.Exceptions
{
    /// <summary>
    /// Raised on connection errors.
    /// </summary>
    public class DocumentManagementException : Exception
    {
        public DocumentManagementException(string message, string details = null, IReadOnlyDictionary<string, string[]> errors = null)
            : base(message)
        {
            Details = details;
            Errors = errors as IReadOnlyDictionary<string, string[]>;
        }

        /// <summary>
        /// Error details, e.g. server exception message. May be null.
        /// </summary>
        public string Details { get; }

        public IReadOnlyDictionary<string, string[]> Errors { get; }
    }
}
