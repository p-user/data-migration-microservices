using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Data_Migration.DataMigration.Features.CsvReport
{
    public class CsvReportService : ICsvReportService
    {
        private readonly ILogger<CsvReportService> _logger;

        public CsvReportService(ILogger<CsvReportService> logger)
        {
            _logger = logger;
        }


        public async Task<string> GenerateImportReportAsync(CsvReportData reportData)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("ddMMyyyy");
                var reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");

                if (!Directory.Exists(reportsDirectory))
                {
                    Directory.CreateDirectory(reportsDirectory);
                }

                var filePath = Path.Combine(reportsDirectory, $"WorkOrder_Import_Summary_{timestamp}.csv");
              

                await GenerateSummaryReportAsync(filePath, reportData);


                _logger.LogInformation("Reporting csv is ready: {Summary}", filePath);

                return reportsDirectory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in csv report");
                throw;
            }
        }

        private async Task GenerateSummaryReportAsync(string filePath, CsvReportData reportData)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };

            await using var writer = new StreamWriter(filePath);
            await using var csv = new CsvWriter(writer, config);

            csv.WriteField("Import Date");
            csv.WriteField(reportData.ImportDate.ToString("dd-MM-yyyy HH:mm:ss"));
            await csv.NextRecordAsync();

            csv.WriteField("Source File");
            csv.WriteField(reportData.SourceFile);
            await csv.NextRecordAsync();

            csv.WriteField("Total Rows Processed");
            csv.WriteField(reportData.TotalRows);
            await csv.NextRecordAsync();

            csv.WriteField("Successfully Inserted");
            csv.WriteField(reportData.SuccessfulRows);
            await csv.NextRecordAsync();

            csv.WriteField("Failed Rows");
            csv.WriteField(reportData.FailedRows);
            await csv.NextRecordAsync();

            csv.WriteField("Duration (seconds)");
            csv.WriteField(reportData.Duration.TotalSeconds.ToString("F2"));
            await csv.NextRecordAsync();

          
            await csv.NextRecordAsync();
            csv.WriteField("Error Type");
            csv.WriteField("Count");
            await csv.NextRecordAsync();

            var errorGroups = reportData.Errors
                .GroupBy(e => GetErrorType(e))
                .OrderByDescending(g => g.Count());

            foreach (var group in errorGroups)
            {
                csv.WriteField(group.Key);
                csv.WriteField(group.Count());
                await csv.NextRecordAsync();
            }
        }

       

        private string GetErrorType(string errorMessage)
        {
            if (errorMessage.Contains("Technician not found"))
                return "Missing Technician";
            if (errorMessage.Contains("Client not found"))
                return "Missing Client";
            if (errorMessage.Contains("Failed to parse"))
                return "Parsing Error";
            if (errorMessage.Contains("Invalid"))
                return "Validation Error";

            return "Other Error";
        }

        
    }

}

