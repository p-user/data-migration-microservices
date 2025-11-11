using Data_Migration.DataMigration.Dtos;

namespace Data_Migration.DataMigration.Features.WorkOrderImport
{
    public interface INotesParserService
    {
        WorkOrderProcessedDto ParseNotes(WorkOrderRawDto rawDto);
    }
}
