namespace ERP_Service.ERP.Features.Technician.CreateTechnician
{
    public class CreateTechnicianEndpoint :ICarterModule
    {
    
     public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/Technician", CreateTechnician)
             .WithName("Create Technician")
             .Produces<CreateTechnicianCommandResponse>(StatusCodes.Status201Created)
             .Produces(StatusCodes.Status400BadRequest)
             .WithTags("Technician");
        }


        private async Task<IResult> CreateTechnician(CreateTechnicianDto dto, ISender sender)
        {
            var command = new CreateTechnicianCommand(dto);
            var response = await sender.Send(command);
            return Results.Created("/Technician", response.TechnicianId );
        }
    }
}
