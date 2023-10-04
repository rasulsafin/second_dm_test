using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Reports;
using Brio.Docs.Reports.Models;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Services
{
    public class ReportService : IReportService
    {
        private readonly DMContext context;
        private readonly DynamicFieldsHelper dynamicFieldsHelper;
        private readonly IStringLocalizer<ReportLocalization> reportLocalizer;
        private readonly ReportGenerator reportGenerator;
        private readonly ILogger<ReportService> logger;

        public ReportService(
            DMContext context,
            DynamicFieldsHelper dynamicFieldsHelper,
            IStringLocalizer<ReportLocalization> reportLocalizer,
            ReportGenerator reportGenerator,
            ILogger<ReportService> logger)
        {
            this.context = context;
            this.dynamicFieldsHelper = dynamicFieldsHelper;
            this.reportLocalizer = reportLocalizer;
            this.reportGenerator = reportGenerator;
            this.logger = logger;
        }

        public Task<IEnumerable<AvailableReportTypeDto>> GetAvailableReportTypes()
        {
            var result = reportGenerator.AvailableReports.Select(x => new AvailableReportTypeDto()
            {
                ID = x.ID,
                Name = x.Name,
                Description = x.Description,
                Fields = x.Fields.Select(x => new AvailableReportFieldDto { Key = x }).ToList(),
            });

            return Task.FromResult(result);
        }

        public async Task<ObjectiveReportCreationResultDto> GenerateReport(string reportTypeId, ReportDto report, string projectDirectory, int userID, string projectName)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogInformation(
                "GenerateReport started for user {UserId} with reportTypeId = {ReportTypeId}, path = {Path}, projectName = {ProjectName} objectiveIds: {@ObjectiveIDs}",
                userID,
                reportTypeId,
                projectDirectory,
                projectName,
                report.Objectives);
            try
            {
                if (!reportGenerator.TryGetReportInfo(reportTypeId, out _))
                    throw new ArgumentValidationException("Unknown report type ID");

                if (report.Objectives == null)
                    throw new ArgumentValidationException("Cannot create report without objectives");

                var date = DateTime.Now;
                var reportFilePath = await ExecuteAndIncrementReportCount(userID, date, async count =>
                {
                     return await GenerateReportCore(reportTypeId, report, date, count, projectDirectory);
                });

                return new ObjectiveReportCreationResultDto()
                {
                    ReportPath = reportFilePath,
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't create report");
                if (ex is ArgumentValidationException || ex is ANotFoundException)
                    throw;

                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        private async Task<string> ExecuteAndIncrementReportCount(int userID, DateTime date, Func<int, Task<string>> generateReport)
        {
            var dateOnly = date.Date;
            var reportCount = await context.ReportCounts.FindAsync(userID);

            string result;
            if (reportCount == null)
            {
                result = await generateReport(1);

                reportCount = new ReportCount() { UserID = userID, Count = 1, Date = dateOnly };
                await context.ReportCounts.AddAsync(reportCount);
            }
            else
            {
                if (reportCount.Date == dateOnly)
                {
                    reportCount.Count += 1;
                }
                else
                {
                    reportCount.Date = dateOnly;
                    reportCount.Count = 1;
                }

                result = await generateReport(reportCount.Count);
            }

            logger.LogDebug("Report Count updating: {@ReportCount}", reportCount);
            await context.SaveChangesAsync();

            return result;
        }

        private async Task<string> GenerateReportCore(
            string reportTypeId,
            ReportDto report,
            DateTime generationDate,
            int reportIndex,
            string projectDirectory)
        {
            var reportID = $"{generationDate:yyyyMMdd}-{reportIndex}";

            var reportDetails = CreateReportDetails(generationDate, reportID, report);
            var objectives = await CreateObjectivesDetails(report, projectDirectory);
            var vm = CreateReportModel(reportDetails, objectives);

            var reportDir = Path.Combine(projectDirectory, "Reports");
            Directory.CreateDirectory(reportDir);
            var reportName = $"{reportLocalizer["Report"]} {reportID}";

            var targetPath = reportGenerator.Generate(reportTypeId, reportDir, reportName, vm);
            logger.LogInformation("Report created ({Path})", targetPath);

            return targetPath;
        }

        private ReportModel CreateReportModel(ReportDetails reportDetails, List<ObjectiveDetails> objectives)
        {
            return new ReportModel()
            {
                ReportInfo = reportDetails ?? throw new ArgumentNullException(nameof(reportDetails)),
                Objectives = objectives ?? throw new ArgumentNullException(nameof(objectives)),
            };
        }

        private ReportDetails CreateReportDetails(DateTime creationTime, string reportNumber, ReportDto reportDto)
        {
            return new ReportDetails
            {
                CreationTime = creationTime,
                ReportNumber = reportNumber,
                Fields = new Dictionary<string, string>(reportDto.Fields),
            };
        }

        private async Task<List<ObjectiveDetails>> CreateObjectivesDetails(ReportDto reportDto, string projectDirectory)
        {
            var objectives = new List<ObjectiveDetails>();
            var screenshotTypes = reportDto.ScreenshotTypes?.Select(x => x?.TrimStart('.')).ToHashSet();

            foreach (var objectiveId in reportDto.Objectives)
            {
                var objective = await GetOrThrowAsync(objectiveId);

                var objectiveModel = new ObjectiveDetails
                {
                    Title = objective.Title,
                    Description = objective.Description,
                    Author = new UserDetails
                    {
                        Name = objective.Author?.Name ?? string.Empty,
                    },
                    CreationTime = objective.CreationDate,
                    DueTime = objective.DueDate,
                    AttachedElements = CreateAttachedElementsDetails(objective).ToList(),
                    AttachedImages = CreateAttachedImagesDetails(objective, projectDirectory)
                        .Where(x => IsScreenshotNeededInReport(screenshotTypes, x))
                        .ToList(),
                    Fields = await CreateDynamicFieldsLookup(objective),
                };

                objectives.Add(objectiveModel);
            }

            logger.LogDebug("Objectives for report: {@Objectives}", objectives);
            return objectives;
        }

        private IEnumerable<AttachedElementDetails> CreateAttachedElementsDetails(Objective objective)
        {
            foreach (var item in objective.BimElements ?? Enumerable.Empty<BimElementObjective>())
            {
                yield return new AttachedElementDetails
                {
                    Name = item.BimElement.ElementName,
                    ProjectName = item.BimElement.ParentName,
                    GlobalID = item.BimElement.GlobalID,
                };
            }
        }

        private IEnumerable<AttachedImageDetails> CreateAttachedImagesDetails(Objective objective, string projectDirectory)
        {
            foreach (var item in objective.Items ?? Enumerable.Empty<ObjectiveItem>())
            {
                var itemInfo = item.Item;
                if (!ReportGenerator.IsSupportedImageExtension(itemInfo.RelativePath))
                    continue;

                var itemPath = Path.Combine(projectDirectory, itemInfo.RelativePath.TrimStart('\\'));
                if (!File.Exists(itemPath))
                {
                    logger.LogWarning("Attached image not found on path {Path}", itemPath);
                    continue;
                }

                yield return new AttachedImageDetails
                {
                    ImagePath = itemPath,
                };
            }
        }

        private async Task<Dictionary<string, string>> CreateDynamicFieldsLookup(Objective objective)
        {
            var ret = new Dictionary<string, string>();

            foreach (var field in objective.DynamicFields ?? Enumerable.Empty<DynamicField>())
            {
                var fieldDto = await dynamicFieldsHelper.BuildObjectDynamicField(field);
                var name = fieldDto.Name;
                string value;
                if (fieldDto.Value is Enumeration en)
                {
                    value = en.Value.Value;
                }
                else
                {
                    value = fieldDto?.Value?.ToString() ?? string.Empty;
                }

                ret[name] = value;
            }

            return ret;
        }

        private bool IsScreenshotNeededInReport(IReadOnlySet<string> needed, AttachedImageDetails image)
        {
            if (needed == null)
                return true; // All screenshots if there are no screenshot types in the DTO.

            return needed.Contains(image.Suffix, StringComparer.OrdinalIgnoreCase) ||
                (image.IsUnknownMode && needed.Contains(null));
        }

        private async Task<Objective> GetOrThrowAsync(ID<ObjectiveDto> objectiveID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Get started for objective {ID}", objectiveID);
            var dbObjective = await context.Objectives
                .Unsynchronized()
                .Include(x => x.Project)
                .Include(x => x.Author)
                .Include(x => x.ObjectiveType)
                .Include(x => x.Location)
                     .ThenInclude(x => x.Item)
                .Include(x => x.DynamicFields)
                     .ThenInclude(x => x.ChildrenDynamicFields)
                .Include(x => x.Items)
                     .ThenInclude(x => x.Item)
                .Include(x => x.BimElements)
                     .ThenInclude(x => x.BimElement)
                .FindOrThrowAsync(x => x.ID, (int)objectiveID);

            logger.LogDebug("Found objective: {@DBObjective}", dbObjective);

            return dbObjective;
        }
    }
}
