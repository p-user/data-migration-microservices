
using ERP_Service.ERP.Features.Technician.CreateTechnician;

namespace ERP_Service.ERP.Features.Client.CreateClient
{
    public class CreateClientEndpoint : ICarterModule
    {
       
      public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/Client", CreateClient)
             .WithName("Create Client")
             .Produces<CreateClientCommandHandlerResponse>(StatusCodes.Status201Created)
             .Produces(StatusCodes.Status400BadRequest)
             .WithTags("Client");
        }


        private async Task<IResult> CreateClient(CreateClientDto dto, ISender sender)
        {
            var command = new CreateClientCommand(dto);
            var response = await sender.Send(command);
            return Results.Created("/Client", response.clientId);
        }
    }
    
}
