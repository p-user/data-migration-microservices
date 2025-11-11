using ERP_Service.ERP.Models;

namespace ERP_Service.ERP.Dtos
{
    public record ClientDto(int Id, string FirstName, string LastName);

    public record CreateClientDto : ClientDto
    {
        public CreateClientDto(int Id, string FirstName, string LastName) : base(Id, FirstName, LastName)
        {
        }
    }
}
