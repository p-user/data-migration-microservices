namespace Data_Migration.DataMigration.Features.CsvReport
{
    public interface ICsvReportService
    {
        Task<string> GenerateImportReportAsync(CsvReportData reportData);
    }
}
