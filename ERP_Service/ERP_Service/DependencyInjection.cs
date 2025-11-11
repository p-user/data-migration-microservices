using ERP_Service.Data;
using ERP_Service.Data.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace ERP_Service
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServicestoErpService(this IServiceCollection services,IConfiguration configuration)
        {

            services.AddScoped<AuditableInterceptor>();

            // Add db context with connection + interceptor
            services.AddDbContext<ERP_Dbcontext>((sp, options) =>
            {
                var auditableInterceptor = sp.GetRequiredService<AuditableInterceptor>();
                options.AddInterceptors(auditableInterceptor);
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));



            });

            return services;
        }

        public static IApplicationBuilder UseServicestoErpServices(this IApplicationBuilder app)
        {


           
            return app;
        }
    }
}
