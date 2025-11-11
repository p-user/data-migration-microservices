using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SharedKernel.Messaging.Extensions
{
    public static class MassTransitExtension
    {
        public static IServiceCollection AddMassTransit(this IServiceCollection _services, IConfiguration _configuration, Assembly assembly)
        {


            _services.AddMassTransit(config =>
            {
                config.SetKebabCaseEndpointNameFormatter();


                config.AddConsumers(assembly);

                config.UsingRabbitMq((context, config) =>
                {
                    config.Host(new Uri(_configuration["MessageBroker:Host"]!), host =>
                    {
                        host.Username(_configuration["MessageBroker:UserName"]);
                        host.Password(_configuration["MessageBroker:Password"]);
                    });



                    config.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));
                    config.ConfigureEndpoints(context);


                });
            });
            return _services;
        }
    }

}
