namespace Data_Migration.DataMigration.Models
{
    public class Client : Entity<int>
    {
       
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
