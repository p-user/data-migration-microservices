using ERP_Service.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Core.Messaging.Events;

namespace ERP_Service.Events.Consumers
{
    public class ClientCreatedEventConsumer : IConsumer<ClientCreatedEvent>
    {

        private readonly ERP_Dbcontext _context;
        private readonly ILogger<ClientCreatedEventConsumer> _logger;

        public ClientCreatedEventConsumer(ERP_Dbcontext context,ILogger<ClientCreatedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<ClientCreatedEvent> context)
        {
            var message = context.Message;

            try
            {
                // Check if client exists 
                var existingClient = await _context.clients.FirstOrDefaultAsync(c => c.FirstName== message.FirstName && c.LastName == message.LastName);

                if (existingClient != null)
                {
                    _logger.LogWarning("Client {Client} already exists",message.FirstName+ message.LastName);
                }
                else
                {
                    // insert
                    var client = Client.Create(message.FirstName, message.LastName);
                    await _context.clients.AddAsync(client);

                    _logger.LogInformation("Created new client {ClientId}", message.FirstName+ message.LastName);
                }

                // Save
                await _context.SaveChangesAsync();

              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error processing ClientMigrated message for ClientId: {ClientId}",message.ClientId);
                throw; 
            }
        }
    }
}
