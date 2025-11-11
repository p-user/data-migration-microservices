
using Data_Migration.DataMigration.Models;
using Microsoft.EntityFrameworkCore;

namespace Data_Migration.Data
{
    public class Data_Migration_Dbcontext : DbContext
    {
        public Data_Migration_Dbcontext(DbContextOptions<Data_Migration_Dbcontext> options) : base(options) { }

        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Technician> Technicians { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            base.OnModelCreating(builder);
        }
    }
}
