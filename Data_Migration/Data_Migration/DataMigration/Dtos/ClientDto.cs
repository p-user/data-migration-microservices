namespace Data_Migration.DataMigration.Dtos
{
    public record ClientDto(string FirstName, string LastName, bool IsPushedToErp = false);
}
