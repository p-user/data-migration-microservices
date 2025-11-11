namespace ERP_Service.ERP.Dtos
{
    public record TechnicianDto(int Id, string FirstName, string LastName);
    public record CreateTechnicianDto : TechnicianDto
    {
        public CreateTechnicianDto(int Id, string FirstName, string LastName) : base(Id, FirstName, LastName)
        {
        }
    }
}
