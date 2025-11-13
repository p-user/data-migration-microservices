using ERP_Service.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Core.Messaging.Events;

namespace ERP_Service.Events.Consumers
{
    public class TechnicianCreatedEventConsumer : IConsumer<TechnicianCreatedEvent>
    {
        private readonly ERP_Dbcontext _context;
        private readonly ILogger<TechnicianCreatedEventConsumer> _logger;

        public TechnicianCreatedEventConsumer(ERP_Dbcontext context, ILogger<TechnicianCreatedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<TechnicianCreatedEvent> context)
        {
            var message = context.Message;
            try
            {
                var existingTechnician = await _context.technicians.FirstOrDefaultAsync(t => t.FirstName==message.FirstName && t.LastName==message.LastName);

                if (existingTechnician != null)
                {
                    _logger.LogWarning("Technician {Technician} already exists,",message.FirstName + message.LastName);

                }
                else
                {
                    var technician = Technician.Create(message.FirstName,message.LastName);


                    await _context.technicians.AddAsync(technician);

                    _logger.LogInformation("Created new technician {Technician}", message.FirstName + message.LastName);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully processed TechnicianCreatedEvent for Technician: {Technician}",message.FirstName + message.LastName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error processing TechnicianMigrated message for Technician: {Technician}", message.FirstName + message.LastName);
                throw;
            }
        }
    }
}
