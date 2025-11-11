using Data_Migration.DataMigration.Dtos;

namespace Data_Migration.DataMigration.Features.WorkOrderImport
{
    public interface IWorkOrderImportService
    {
        Task<ImportResultDto> ImportWorkOrdersAsync(string filePath);
    }
}
