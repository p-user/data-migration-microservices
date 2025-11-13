
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;
using System.Text.Json;

namespace SharedKernel.Core.Extensions
{
    public static class HealthCheckExtension
    {
        public static IServiceCollection AddApplicationHealthChecks<TDbContext>(this IServiceCollection services, string serviceName) where TDbContext : DbContext
        {
            services.AddHealthChecks()


                // Database health check
                .AddDbContextCheck<TDbContext>(name: "database",failureStatus: HealthStatus.Unhealthy, tags: new[] { "db", "sql" })

                // RabbitMQ health check
                .AddRabbitMQ(name: "rabbitmq",failureStatus: HealthStatus.Unhealthy, tags: new[] { "messaging", "rabbitmq" })

                // Memory health check
                .AddCheck("memory", () =>
                {
                    var allocated = GC.GetTotalMemory(forceFullCollection: false);
                    var threshold = 1024L * 1024L * 1024L; // 1 GB

                    return allocated < threshold
                        ? HealthCheckResult.Healthy($"Memory usage: {allocated / 1024 / 1024} MB")
                        : HealthCheckResult.Degraded($"Memory usage high: {allocated / 1024 / 1024} MB");
                }, tags: new[] { "memory" })

                // Service info
                .AddCheck("service-info", () => HealthCheckResult.Healthy($"Service: {serviceName}, Version: 1.0.0"),
                    tags: new[] { "info" });

            return services;
        }

        /// <summary>
        /// Maps health check endpoints with detailed JSON responses
        /// </summary>
        public static IApplicationBuilder UseApplicationHealthChecks(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = WriteHealthCheckResponse
            });

            app.UseHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("rabbitmq"),
                ResponseWriter = WriteHealthCheckResponse
            });

            app.UseHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false, // Minimal check - just returns 200 if app is running
                ResponseWriter = WriteHealthCheckResponse
            });

            return app;
        }


        private static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonWriterOptions { Indented = true };
            using var memoryStream = new MemoryStream();
            using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("status", report.Status.ToString());
                jsonWriter.WriteString("timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                jsonWriter.WriteNumber("totalDuration", report.TotalDuration.TotalMilliseconds);

                jsonWriter.WriteStartObject("checks");
                foreach (var (key, value) in report.Entries)
                {
                    jsonWriter.WriteStartObject(key);
                    jsonWriter.WriteString("status", value.Status.ToString());
                    jsonWriter.WriteString("description", value.Description);
                    jsonWriter.WriteNumber("duration", value.Duration.TotalMilliseconds);

                    if (value.Exception != null)
                    {
                        jsonWriter.WriteString("exception", value.Exception.Message);
                    }

                    if (value.Data.Any())
                    {
                        jsonWriter.WriteStartObject("data");
                        foreach (var (dataKey, dataValue) in value.Data)
                        {
                            jsonWriter.WritePropertyName(dataKey);
                            JsonSerializer.Serialize(jsonWriter, dataValue, dataValue?.GetType() ?? typeof(object));
                        }
                        jsonWriter.WriteEndObject();
                    }

                    jsonWriter.WriteEndObject();
                }
                jsonWriter.WriteEndObject();

                jsonWriter.WriteEndObject();
            }

            return context.Response.WriteAsync(Encoding.UTF8.GetString(memoryStream.ToArray()));
        }
    }
}
