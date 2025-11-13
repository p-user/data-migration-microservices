namespace Data_Migration.Utilities
{
    public interface IDataMigrationService
    {
        Task MigrateClientsAsync(CancellationToken cancellationToken = default);
        Task MigrateWorkOrdersAsync(CancellationToken cancellationToken = default);
        Task MigrateTechniciansAsync(CancellationToken cancellationToken = default);
    }
}
