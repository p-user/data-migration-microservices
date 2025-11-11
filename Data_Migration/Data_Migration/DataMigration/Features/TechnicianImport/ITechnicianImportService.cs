namespace Data_Migration.DataMigration.Features.TechnicianImport
{
    public interface ITechnicianImportService
    {
        Task<ImportResultDto> ImportTechniciansAsync(string filePath);
        Task<Dictionary<string, int>> GetTechnicianLookupAsync();
    }
}
