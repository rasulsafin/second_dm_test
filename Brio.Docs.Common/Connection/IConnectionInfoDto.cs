using System.Collections.Generic;

namespace Brio.Docs.Common.Dtos
{
    /// <summary>
    /// Interface for Automapper to collect all ConnectionInfoDtos under one umbrella.
    /// </summary>
    public interface IConnectionInfoDto
    {
        /// <summary>
        /// Property to be mapped.
        /// </summary>
        IDictionary<string, string> AuthFieldValues { get; set; }
    }
}
