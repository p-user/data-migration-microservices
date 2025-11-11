
using AutoMapper;
using ERP_Service.Data;

namespace ERP_Service.ERP.Features.Client.CreateClient
{
    public record CreateClientCommand(CreateClientDto dto): IRequest<CreateClientCommandHandlerResponse>;
    public record CreateClientCommandHandlerResponse(int clientId);
    public class CreateClientCommandHandler(ERP_Dbcontext _context, IMapper _mapper) : IRequestHandler<CreateClientCommand, CreateClientCommandHandlerResponse>
    {
        public async Task<CreateClientCommandHandlerResponse> Handle(CreateClientCommand request, CancellationToken cancellationToken)
        {
            //ToDo: Additional dto validations should be applied/checked

            var toBeInserted = Models.Client.Create(request.dto.FirstName, request.dto.LastName);
            var insertedClient = await _context.clients.AddAsync(toBeInserted, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return new CreateClientCommandHandlerResponse(clientId: insertedClient.Entity.Id);

        }
    }
}
