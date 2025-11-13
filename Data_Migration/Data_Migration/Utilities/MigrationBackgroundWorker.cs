namespace Data_Migration.Utilities
{
    public class MigrationBackgroundWorker : BackgroundService
    {

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MigrationBackgroundWorker> _logger;

       
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(2);


        public MigrationBackgroundWorker(IServiceProvider serviceProvider,ILogger<MigrationBackgroundWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           

            //let some time  for the application to be ready
           // await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Migration Background Worker cancelled during startup");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessMigrationsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during migration processing");
                }

               
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Migration Background Worker stopped");
        }


        private async Task ProcessMigrationsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var migrationService = scope.ServiceProvider.GetRequiredService<IDataMigrationService>();

            try
            {
               
                // Process clients
                await migrationService.MigrateClientsAsync(cancellationToken);


                // Process technicians
                await migrationService.MigrateTechniciansAsync(cancellationToken);

                // Process workOrders
                //let some time  for the application to be ready
                await Task.Delay(TimeSpan.FromSeconds(5));
                await migrationService.MigrateWorkOrdersAsync(cancellationToken);
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during migration batch processing");
                throw;
            }
        }
    }
}
