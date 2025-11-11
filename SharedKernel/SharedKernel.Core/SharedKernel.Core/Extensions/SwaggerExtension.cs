using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;


namespace SharedKernel.Core.Extensions
{
    public static class SwaggerExtension
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services, string apiTitle = "API", string apiVersion = "v1")
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(apiVersion, new OpenApiInfo
                {
                    Title = apiTitle,
                    Version = apiVersion
                });


                var xmlFile = $"{System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            });

            return services;
        }

        public static IApplicationBuilder MapSwaggerUI(this WebApplication app, string apiVersion = "v1")
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", $"API {apiVersion}");
                options.RoutePrefix = string.Empty;
            });

            return app;


        }

    }
}
