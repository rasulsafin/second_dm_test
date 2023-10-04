using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Services;

namespace Brio.Docs.HttpConnection.Services
{
    internal class ReportService : ServiceBase, IReportService
    {
        private static readonly string PATH = "Report";

        public ReportService(Connection connection)
            : base(connection)
        {
        }

        public async Task<ObjectiveReportCreationResultDto> GenerateReport(string reportTypeId, ReportDto report, string projectDirectory, int userID, string projectName)
            => await Connection.PostObjectJsonQueryAsync<ReportDto, ObjectiveReportCreationResultDto>($"{PATH}/create",
                $"reportTypeId={{0}}&projectDirectory={{1}}&userID={{2}}&projectName={{3}}",
                new object[] { reportTypeId, projectDirectory, userID, projectName },
                report);

        public async Task<IEnumerable<AvailableReportTypeDto>> GetAvailableReportTypes()
            => await Connection.GetDataAsync<IEnumerable<AvailableReportTypeDto>>($"{PATH}/reportTypes");
    }
}
