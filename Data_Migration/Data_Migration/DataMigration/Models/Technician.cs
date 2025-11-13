namespace Data_Migration.DataMigration.Models
{
    public class Technician : Entity<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsPushedToErp { get; set; }=false;
    }
}
