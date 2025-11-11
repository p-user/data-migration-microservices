
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace Data_Migration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServicestoDataMigration(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<AuditableInterceptor>();

            // Add db context with connection + interceptor
            services.AddDbContext<Data_Migration_Dbcontext>((sp, options) =>
            {
                var auditableInterceptor = sp.GetRequiredService<AuditableInterceptor>();
                options.AddInterceptors(auditableInterceptor);
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));



            });
            // Core services
            services.AddSingleton<IExcelReaderService, ParallelExcelReaderService>();
            services.AddScoped<IClientImportService, ClientImportService>();
            services.AddScoped<ITechnicianImportService, TechnicianImportService>();
            services.AddScoped<IWorkOrderImportService, WorkOrderImportService>();
            services.AddScoped<INotesParserService, NotesParserService>();
            services.AddScoped<IBulkInsertService, BulkInsertService>();
            ExcelPackage.License.SetNonCommercialPersonal("My Name");
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            return services;
        }
    }
}
