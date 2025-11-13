
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Emit;

namespace ERP_Service.Data
{
    public class ERP_Dbcontext : DbContext
    {
        public ERP_Dbcontext(DbContextOptions<ERP_Dbcontext> options) : base(options) { }

        public DbSet<Client> clients => Set<Client>();
        public DbSet<WorkOrder> workOrders => Set<WorkOrder>();
        public DbSet<Technician> technicians => Set<Technician>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd(); 
            });

            builder.Entity<WorkOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd(); 
            });

            builder.Entity<Technician>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd(); 
            });

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            builder.AddOutboxStateEntity();
            builder.AddInboxStateEntity();
            builder.AddOutboxMessageEntity();
            base.OnModelCreating(builder);
        }
    }
}
