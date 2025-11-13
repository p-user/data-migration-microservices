using AutoMapper;
using ERP_Service.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP_Service.ERP.Features.WorkOrder
{

    public record CreateWorkOrderCommand(CreateWorkOrderDto dto) : IRequest<CreateWorkOrderCommandResponse>;

    public record CreateWorkOrderCommandResponse(int Id, int WorkOrderId);

    public class CreateWorkOrderCommandHandler(ERP_Dbcontext _context, IMapper _mapper) : IRequestHandler<CreateWorkOrderCommand, CreateWorkOrderCommandResponse>
    {
        public async Task<CreateWorkOrderCommandResponse> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
        {
            // Verify Client existence
            var clientExists = await _context.clients.AnyAsync(c => c.Id == request.dto.ClientId, cancellationToken);
            if (!clientExists)
            {
                throw new ArgumentException($"Client with ID {request.dto.ClientId} does not exist.", nameof(request.dto.ClientId));
            }

            // Verify Technician existence
            var technicianExists = await _context.technicians.AnyAsync(t => t.Id == request.dto.TechnicianId, cancellationToken);
            if (!technicianExists)
            {
                throw new ArgumentException($"Technician with ID {request.dto.TechnicianId} does not exist.", nameof(request.dto.TechnicianId));
            }


            var nextWorkOrderId = await _context.workOrders.AnyAsync(cancellationToken)
                ? await _context.workOrders.MaxAsync(wo => wo.Id, cancellationToken) + 1
                : 1;

            var toBeInserted = Models.WorkOrder.Create(
                request.dto.ClientId,
                request.dto.TechnicianId,
                request.dto.Notes,
                request.dto.Total,
                request.dto.WorkDate
            );

            var insertedWorkOrder = await _context.workOrders.AddAsync(toBeInserted, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreateWorkOrderCommandResponse(Id: insertedWorkOrder.Entity.Id, WorkOrderId: insertedWorkOrder.Entity.Id);
        }
    }
}
