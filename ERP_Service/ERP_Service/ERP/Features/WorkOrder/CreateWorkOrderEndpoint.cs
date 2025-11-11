using ERP_Service.ERP.Features.Client.CreateClient;

namespace ERP_Service.ERP.Features.WorkOrder
{
    public class CreateWorkOrderEndpoint: ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/WorkOrder", CreateWorkOrder)
             .WithName("Create WorkOrder")
             .Produces<CreateWorkOrderCommandResponse>(StatusCodes.Status201Created)
             .Produces(StatusCodes.Status400BadRequest)
             .WithTags("WorkOrder");
        }


        private async Task<IResult> CreateWorkOrder(CreateWorkOrderDto dto, ISender sender)
        {
            var command = new CreateWorkOrderCommand(dto);
            var response = await sender.Send(command);
            return Results.Created("/Client", response.Id);
        }
    }
}

