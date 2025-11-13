using Microsoft.EntityFrameworkCore.Design;

namespace Data_Migration.Data
{
    public class Data_Migration_DbcontextFactory : IDesignTimeDbContextFactory<Data_Migration_Dbcontext>
    {
        public Data_Migration_Dbcontext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.Develoment.json", optional: true)
          .AddJsonFile("appsettings.json")
          .Build();


            var optionsBuilder = new DbContextOptionsBuilder<Data_Migration_Dbcontext>();


            var connectionString = configuration.GetConnectionString("DefaultConnection");


            optionsBuilder.UseSqlServer(connectionString, sqlServerOptions =>
            {

                sqlServerOptions.MigrationsAssembly(typeof(Data_Migration_Dbcontext).Assembly.FullName);
            });


            return new Data_Migration_Dbcontext(optionsBuilder.Options);
        }
    }
    
}
