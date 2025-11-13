using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ERP_Service.Data
{
    public class ERP_DbcontextFactory : IDesignTimeDbContextFactory<ERP_Dbcontext>
    {
        public ERP_Dbcontext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Develoment.json", optional: true)
            .AddJsonFile("appsettings.json")
            .Build();


            var optionsBuilder = new DbContextOptionsBuilder<ERP_Dbcontext>();


            var connectionString = configuration.GetConnectionString("DefaultConnection");


            optionsBuilder.UseSqlServer(connectionString, sqlServerOptions =>
            {

                sqlServerOptions.MigrationsAssembly(typeof(ERP_Dbcontext).Assembly.FullName);
            });


            return new ERP_Dbcontext(optionsBuilder.Options);
        }
    }
    
}
