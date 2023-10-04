using System.Collections.Generic;

namespace Brio.Docs.Common.Dtos
{
    /// <summary>
    /// Interface for Automapper to collect all ConnectionTypeDtos under one umbrella.
    /// </summary>
    public interface IConnectionTypeDto
    {
        /// <summary>
        /// Property to be mapped.
        /// </summary>
        IDictionary<string, string> AppProperties { get; set; }
    }
}
