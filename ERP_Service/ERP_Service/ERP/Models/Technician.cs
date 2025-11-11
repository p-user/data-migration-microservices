
namespace ERP_Service.ERP.Models
{
    public class Technician : Entity<int>
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();

        private Technician() { }

        public static Technician Create(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));
            }
            if (firstName.Length > 100)
            {
                throw new ArgumentException("First name cannot exceed 100 characters.", nameof(firstName));
            }
            if (lastName.Length > 100)
            {
                throw new ArgumentException("Last name cannot exceed 100 characters.", nameof(lastName));
            }

            return new Technician
            {
                FirstName = firstName,
                LastName = lastName
            };
        }
    }

}
