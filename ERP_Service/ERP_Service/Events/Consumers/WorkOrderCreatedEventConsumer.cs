using ERP_Service.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Core.Messaging.Events;

namespace ERP_Service.Events.Consumers
{
    public class WorkOrderCreatedEventConsumer : IConsumer<WorkOrderCreatedEvent>
    {
        private readonly ERP_Dbcontext _context;
        private readonly ILogger<WorkOrderCreatedEventConsumer> _logger;

        public WorkOrderCreatedEventConsumer(ERP_Dbcontext context, ILogger<WorkOrderCreatedEventConsumer> logger)
        {
            _context=context;
            _logger=logger;
        }
        public async Task Consume(ConsumeContext<WorkOrderCreatedEvent> context)
        {
            var message = context.Message;



            try
            {

               
                // Check if work order already exists
                //Pytje : ca ndodh nese kemi dy work order me te njejtin client dhe technician ?
                var existingWorkOrder = await _context.workOrders.FirstOrDefaultAsync(wo =>
                wo.Technician.FirstName == message.TechnicianFirstName &&
                wo.Technician.LastName == message.TechnicianLastName &&
                wo.Client.FirstName == message.ClientFirstName &&
                wo.Client.LastName == message.ClientLastName &&
                wo.WorkDate == message.ServiceDate
                );

                if (existingWorkOrder != null)
                {


                    _logger.LogWarning("WorkOrder {workOrder} already exists,", message.ToString);
                }
                else
                {
                    // Verify client exists
                    var clientExists = await _context.clients.FirstOrDefaultAsync(c => c.FirstName == message.ClientFirstName && c.LastName== message.ClientLastName);

                    if (clientExists is null)
                    {
                        _logger.LogError("Client {Client} not found for WorkOrder {WorkOrderId}, will retry...", message.ClientFirstName+ message.ClientLastName, message.WorkOrderId);
                        throw new InvalidOperationException($"Client {message.ClientFirstName+ message.ClientLastName} not found");
                    }

                    // Verify technician exists
                    var technicianExists = await _context.technicians.FirstOrDefaultAsync(c => c.FirstName == message.TechnicianFirstName && c.LastName== message.TechnicianLastName);

                    if (clientExists is null)
                    {
                        _logger.LogError("Technician {Technician} not found for WorkOrder {WorkOrderId}, will retry...", message.TechnicianFirstName+ message.TechnicianLastName, message.WorkOrderId);
                        throw new InvalidOperationException($"Technician {message.TechnicianFirstName+ message.TechnicianLastName} not found");
                    }

                    var workOrder = WorkOrder.Create(clientExists.Id, technicianExists.Id, "Inserted from excel migration", message.Total, message.ServiceDate);
                    await _context.workOrders.AddAsync(workOrder);

                    _logger.LogInformation("Created new work order {WorkOrderId}", workOrder.Id);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully consumed workorderCreatedEvent for WorkOrderId: {WorkOrderId}", message.WorkOrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing WorkOrderMigrated message for WorkOrderId: {WorkOrderId}", message.WorkOrderId);
                throw;
            }
        }
    }
}
