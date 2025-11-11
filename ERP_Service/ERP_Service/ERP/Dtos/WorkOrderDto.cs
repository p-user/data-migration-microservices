using ERP_Service.ERP.Dtos;

namespace ERP_Service.ERP.Dtos
{
    public record WorkOrderDto(
    int Id,
    int ClientId,
    int TechnicianId,
    string Notes,
    decimal Total,
    DateTime WorkDate,
    bool IsValid
    
);
}

public record CreateWorkOrderDto : WorkOrderDto
{
    public CreateWorkOrderDto(
        int Id,
        int ClientId,
        int TechnicianId,
        string Notes,
        decimal Total,
        DateTime WorkDate,
        bool IsValid
    ) : base(Id,ClientId, TechnicianId, Notes, Total, WorkDate, IsValid)
    {
    }
}
