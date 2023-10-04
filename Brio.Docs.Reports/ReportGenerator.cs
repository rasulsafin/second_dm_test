using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Brio.Docs.Reports.Models;
using CsvHelper;
using CsvHelper.Configuration;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharpDocx;

namespace Brio.Docs.Reports
{
    public enum ReportType
    {
        Report = 0,
        Table = 1,
    }

    public class ReportGenerator
    {
        private static readonly string REPORTS_RES_FOLDER = @"ReportResources";
        private static readonly string REPORTS_MANIFEST = @"reports.json";

        private readonly Dictionary<string, ReportInfo> reports = new Dictionary<string, ReportInfo>();
        private readonly string reportResourcesFolder;
        private readonly ILogger logger;

        public ReportGenerator(ILogger<ReportGenerator> logger)
        {
            this.logger = logger;
            var currentExecutablePath = Assembly.GetEntryAssembly().Location;
            var currentExecutableFolder = Path.GetDirectoryName(currentExecutablePath);
            reportResourcesFolder = Path.Combine(currentExecutableFolder, REPORTS_RES_FOLDER);
            var reportsManifestPath = Path.Combine(reportResourcesFolder, REPORTS_MANIFEST);

            LoadReportsManifest(reportsManifestPath);
        }

        public IReadOnlyCollection<ReportInfo> AvailableReports => reports.Values;

        public static bool IsSupportedImageExtension(string filePath)
            => SharpImage.ImageInfo.GetType(filePath) != SharpImage.ImageInfo.Type.Unknown;

        public bool TryGetReportInfo(string reportTypeId, out ReportInfo info)
            => reports.TryGetValue(reportTypeId, out info);

        public string Generate(string reportTypeId, string outFolder, string reportName, ReportModel vm)
        {
            if (!reports.TryGetValue(reportTypeId, out var reportInfo))
                throw new ArgumentException("Can not find report template by ID: {ID}", reportTypeId);

            string filePath;

            switch (reportInfo.ReportType)
            {
                case ReportType.Report:
                    filePath = Path.Combine(outFolder, $"{reportName}.docx");
                    CreateDocxReport(filePath, vm, reportInfo);
                    break;

                case ReportType.Table:
                    filePath = Path.Combine(outFolder, $"{reportName}.csv");
                    CreateCsvReport(filePath, vm);
                    break;

                default:
                    throw new InvalidOperationException($"Unable to create report of type {reportInfo.ReportType}");
            }

            return filePath;
        }

        private void CreateCsvReport(string outFilePath, ReportModel vm)
        {
            var attachedElements = vm.Objectives.SelectMany(x => x.AttachedElements).ToList();

            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
            };

            using (var writer = new StreamWriter(outFilePath))
            using (var csv = new CsvWriter(writer, configuration))
            {
                csv.Context.RegisterClassMap<CsvRow>();
                csv.WriteHeader<AttachedElementDetails>();
                csv.NextRecord();
                csv.WriteRecords(attachedElements);
            }
        }

        private void CreateDocxReport(
            string outFilePath,
            ReportModel vm,
            ReportInfo info)
        {
            var templateFilePath = info.TemplateFilePath;
            File.Delete(outFilePath);

            var doc = DocumentFactory.Create(templateFilePath, vm);
            doc.Generate(outFilePath);

            try
            {
                MakeToc(outFilePath);
            }
            catch (Exception ex)
            {
                logger.LogError($"TOC can't be created:\n{ex}");
            }
        }

        private void MakeToc(string path)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(path, true))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                string headingStyleId = string.Empty;

                var stylePart = wordDoc.MainDocumentPart.StyleDefinitionsPart;
                if (stylePart != null)
                {
                    using (var reader = XmlReader.Create(stylePart.GetStream(FileMode.Open, FileAccess.Read)))
                    {
                        var document = XDocument.Load(reader);
                        var styles = document.Elements();
                        var paragraphs = document.
                            Root.
                            Elements("{http://schemas.openxmlformats.org/wordprocessingml/2006/main}style").
                            Where(x => x.Attribute("{http://schemas.openxmlformats.org/wordprocessingml/2006/main}type").Value == "paragraph");

                        foreach (var paragraph in paragraphs)
                        {
                            var isHeading1 = paragraph.
                                Element("{http://schemas.openxmlformats.org/wordprocessingml/2006/main}name").
                                Attribute("{http://schemas.openxmlformats.org/wordprocessingml/2006/main}val").
                                Value.
                                ToLower() == "heading 1";

                            if (isHeading1)
                            {
                                headingStyleId = paragraph.Attribute("{http://schemas.openxmlformats.org/wordprocessingml/2006/main}styleId").Value;
                                break;
                            }
                        }
                    }
                }

                var headers = body.
                    Descendants<ParagraphProperties>().
                    ToList().
                    Where(x => x.ParagraphStyleId != null).
                    Where(x => x.ParagraphStyleId.Val == headingStyleId);

                var toc = body.
                    ChildElements.Where(x => x.InnerText.Contains("TOC \\o")).
                    FirstOrDefault();

                if (toc == null)
                    return;

                foreach (var hyperlink in toc.Descendants<Hyperlink>())
                {
                    hyperlink.Remove();
                }

                var lastChild = toc;
                var bookmarkId = 1;

                foreach (var header in headers)
                {
                    var bookmarksStart = header.Parent.Descendants<BookmarkStart>().First();
                    var bookmarksEnd = header.Parent.Descendants<BookmarkEnd>().First();

                    bookmarksStart.Id = $"{bookmarkId}";
                    bookmarksEnd.Id = $"{bookmarkId}";
                    bookmarksStart.Name = $"_TOC{bookmarkId}";
                    bookmarkId++;

                    var paragraph = new Paragraph()
                    {
                        InnerXml = $@"
                        <w:pPr>
                            <w:pStyle w:val=""para19""/>
                            <w:tabs defTabSz=""708"">
                                <w:tab w:val=""right"" w:pos=""9355"" w:leader=""dot""/>
                            </w:tabs>
                        </w:pPr>
                        <w:hyperlink w:anchor=""{bookmarksStart.Name}"" w:history=""1"">
                            <w:r>
                                <w:t>{header.Parent.InnerText}</w:t>
                                <w:tab/>
                                <w:t>?</w:t>
                            </w:r>
                        </w:hyperlink>",
                    };

                    body.InsertAfter(paragraph, lastChild);

                    lastChild = paragraph;
                }

                wordDoc.Save();
            }
        }

        private void LoadReportsManifest(string path)
        {
            var info = ReadReportsManifest(path);
            if (info.Reports == null)
                return;

            foreach (var report in info.Reports)
            {
                if (IsReportInfoValid(report))
                {
                    if (report.ReportType == ReportType.Report)
                    {
                        var templatePath = Path.Combine(reportResourcesFolder, report.TemplateFilePath);
                        if (!File.Exists(templatePath))
                        {
                            logger.LogWarning("Template file in report {ID} does not exist: {Path}", report.ID, templatePath);
                            continue;
                        }

                        report.TemplateFilePath = templatePath;
                    }

                    reports.Add(report.ID, report);
                }
            }
        }

        private ReportsInfo ReadReportsManifest(string path)
        {
            if (!File.Exists(path))
                logger.LogWarning("File not found: {Path}", path);

            try
            {
                var content = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<ReportsInfo>(content);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read {Path}: {Message}", REPORTS_MANIFEST, ex.Message);
                return new ReportsInfo();
            }
        }

        private bool IsReportInfoValid(ReportInfo report)
        {
            if (string.IsNullOrEmpty(report.ID))
            {
                logger.LogWarning($"Report type ID should not be empty");
                return false;
            }

            if (reports.ContainsKey(report.ID))
            {
                logger.LogWarning("Duplicate report type ID: {ID}", report.ID);
                return false;
            }

            if (string.IsNullOrEmpty(report.Name))
            {
                logger.LogWarning("Report type {ID} has no Name", report.ID);
                return false;
            }

            if (report.ReportType != ReportType.Table && string.IsNullOrEmpty(report.TemplateFilePath))
            {
                logger.LogWarning("Template file path is not specified for report type {ID}", report.ID);
                return false;
            }

            return true;
        }

        public class ReportsInfo
        {
            public List<ReportInfo> Reports { get; set; } = new List<ReportInfo>();
        }

        public class ReportInfo
        {
            public string ID { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string TemplateFilePath { get; set; }

            public ReportType ReportType { get; set; }

            public List<string> Fields { get; set; } = new List<string>();
        }

        public class CsvRow : ClassMap<AttachedElementDetails>
        {
            public CsvRow()
            {
                Map(m => m.ProjectName).Index(0).Name("Project name");
                Map(m => m.Name).Index(1).Name("Name");
                Map(m => m.GlobalID).Index(2).Name("Id");
            }
        }
    }
}
