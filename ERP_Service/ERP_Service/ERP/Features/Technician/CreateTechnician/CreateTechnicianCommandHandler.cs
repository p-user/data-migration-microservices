using AutoMapper;
using ERP_Service.Data;

namespace ERP_Service.ERP.Features.Technician.CreateTechnician
{
     public record CreateTechnicianCommand(CreateTechnicianDto dto) : IRequest<CreateTechnicianCommandResponse>;

    public record CreateTechnicianCommandResponse(int TechnicianId);

    public class CreateTechnicianCommandHandler(ERP_Dbcontext _context, IMapper _mapper) : IRequestHandler<CreateTechnicianCommand, CreateTechnicianCommandResponse>
    {
        public async Task<CreateTechnicianCommandResponse> Handle(CreateTechnicianCommand request, CancellationToken cancellationToken)
        {
            var toBeInserted = Models.Technician.Create(request.dto.FirstName, request.dto.LastName);
            var insertedTechnician = await _context.technicians.AddAsync(toBeInserted, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return new CreateTechnicianCommandResponse(TechnicianId: insertedTechnician.Entity.Id);
        }
    }
}
