using Data_Migration.Data;
using MassTransit;
using SharedKernel.Core.Messaging.Events;

namespace Data_Migration.Utilities
{
    public class DataMigrationService : IDataMigrationService
    {
        private readonly Data_Migration_Dbcontext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<DataMigrationService> _logger;

        public DataMigrationService(Data_Migration_Dbcontext context, IPublishEndpoint publishEndpoint, ILogger<DataMigrationService> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }


        public async Task MigrateClientsAsync(CancellationToken cancellationToken = default)
        {
            var batchId = Guid.NewGuid();
            var successful = 0;
            var failed = 0;

            try
            {
                // getting all clients that are not pushed yet
                var clients = await _context.Clients.Where(c => !c.IsPushedToErp)
                    .Take(10)
                    .ToListAsync(cancellationToken);

                foreach (var client in clients)
                {
                    try
                    {
                        // publish message, this will be stored in the Outbox table
                        await _publishEndpoint.Publish(new ClientCreatedEvent
                        {
                            ClientId = client.Id,
                            FirstName = client.FirstName,
                            LastName = client.LastName,
                        }, cancellationToken);

                        // mark as done
                        client.IsPushedToErp = true;
                        successful++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish client {ClientId}", client.Id);
                        failed++;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during client migration");
                throw;
            }
        }

        public async Task MigrateWorkOrdersAsync(CancellationToken cancellationToken = default)
        {
            var batchId = Guid.NewGuid();
            var successful = 0;
            var failed = 0;

            try
            {
                var workOrders = await _context.WorkOrders
                    .Where(wo => !wo.IsPushedToErp)
                    .Take(10)
                    .Include(wo => wo.Client)
                    .Include(wo => wo.Technician)
                    .ToListAsync(cancellationToken);

               

                foreach (var workOrder in workOrders)
                {
                    try
                    {
                        await _publishEndpoint.Publish(new WorkOrderCreatedEvent
                        {
                            WorkOrderId = workOrder.Id,
                            ClientFirstName = workOrder.Client.FirstName,
                            ClientLastName = workOrder.Client.LastName,
                            Total = workOrder.Total,
                            TechnicianFirstName = workOrder.Technician.FirstName,
                            TechnicianLastName = workOrder.Technician.LastName,
                            ServiceDate = workOrder.ServiceDate,
                        }, cancellationToken);

                        workOrder.IsPushedToErp = true;
                        successful++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish work order {WorkOrderId}", workOrder.Id);
                        failed++;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Work order migration batch completed. Successful: {Successful}, Failed: {Failed}", successful, failed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during work order migration");
                throw;
            }
        }

        public async Task MigrateTechniciansAsync(CancellationToken cancellationToken = default)
        {
            var batchId = Guid.NewGuid();
            var successful = 0;
            var failed = 0;

            try
            {
                var technicians = await _context.Technicians
                    .Where(t => !t.IsPushedToErp)
                    .Take(100)
                    .ToListAsync(cancellationToken);

                foreach (var technician in technicians)
                {
                    try
                    {
                        await _publishEndpoint.Publish(new TechnicianCreatedEvent
                        {
                            TechnicianId = technician.Id,
                            FirstName = technician.FirstName,
                            LastName = technician.LastName,
                        }, cancellationToken);

                        technician.IsPushedToErp = true;
                        successful++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish technician {TechnicianId}", technician.Id);
                        failed++;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during technician migration");
                throw;
            }
        }
    }
}
