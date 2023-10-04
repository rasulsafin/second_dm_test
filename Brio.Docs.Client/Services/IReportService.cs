using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;

namespace Brio.Docs.Client.Services
{
    /// <summary>
    /// Service responsible for reports generation.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Get information about available report types and required fields.
        /// </summary>
        /// <returns>A collection of available report types</returns>
        Task<IEnumerable<AvailableReportTypeDto>> GetAvailableReportTypes();

        /// <summary>
        /// Generate report about selected objectives.
        /// </summary>
        /// <param name="reportTypeId">Report type identifier.</param>
        /// <param name="report">Report.</param>
        /// <param name="projectDirectory">Path to report storage.</param>
        /// <param name="userID">ID of the user, who generates the report.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <returns>Object representing the result of report creation process.</returns>
        /// <exception cref="ANotFoundException">Thrown when one of the objectives not found.</exception>
        /// <exception cref="ArgumentValidationException">Thrown when list of objectives is empty.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ObjectiveReportCreationResultDto> GenerateReport(string reportTypeId, ReportDto report, string projectDirectory, int userID, string projectName);
    }
}
