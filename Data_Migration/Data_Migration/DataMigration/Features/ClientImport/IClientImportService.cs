
namespace Data_Migration.DataMigration.Features.ClientImport
{
    public interface IClientImportService
    {
        Task<ImportResultDto> ImportClientsAsync(string filePath);
    }
}
