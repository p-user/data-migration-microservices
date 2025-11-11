namespace Data_Migration.DataMigration.Features.WorkOrderImport
{
    public interface IBulkInsertService
    {

        Task BulkInsertAsync<T>(List<T> items, string tableName) where T : class;
    }
}

