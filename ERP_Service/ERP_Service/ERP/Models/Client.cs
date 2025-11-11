using SharedKernel.Core.DDD;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_Service.ERP.Models
{
    public class Client : Entity<int>
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();


        private Client() { }
     
        public static Client Create(string firstName, string lastName)
        {
            //Validations
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));
            }
           


            //New Client
            return new Client
            {
                FirstName = firstName,
                LastName = lastName
            };
        }

       
    }
}
