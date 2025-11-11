

using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Core.Behaviors;
using System.Reflection;

namespace SharedKernel.Core.Extensions
{
    public static class MediatRExtension
    {
        public static IServiceCollection AddMediatRExtension(this IServiceCollection services, Assembly assembly)
        {

            
                services.AddMediatR(s =>
                {
                    s.RegisterServicesFromAssembly(assembly);
                    s.AddOpenBehavior(typeof(LoggingBehavior<,>));

                });

            

            return services;
        }
    }
}
