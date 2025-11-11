
using Carter;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SharedKernel.Core.Extensions
{
    public static class CarterExtension
    {

        public static IServiceCollection AddCarter(this IServiceCollection services, Assembly assembly)
        {
            services.AddCarter(configurator: config =>
            {
                
                    var modules = assembly.GetTypes().Where(f => f.IsAssignableTo(typeof(ICarterModule))).ToArray();
                    config.WithModules(modules);
               
            });
            return services;
        }
    }
}
