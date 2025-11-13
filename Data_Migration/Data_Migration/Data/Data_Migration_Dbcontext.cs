
using Data_Migration.DataMigration.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

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

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            builder.AddOutboxStateEntity();
            builder.AddInboxStateEntity();
            builder.AddOutboxMessageEntity();
            base.OnModelCreating(builder);

        }
    }
}
