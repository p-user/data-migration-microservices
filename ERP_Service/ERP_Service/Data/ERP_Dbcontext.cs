
using Microsoft.EntityFrameworkCore;

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
            builder.HasDefaultSchema("ERP");
            base.OnModelCreating(builder);
        }
    }
}
