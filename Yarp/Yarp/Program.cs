var builder = WebApplication.CreateBuilder(args);


builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("gateway", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Yarp up and running"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ERP System API Gateway",
        Version = "v1",
        Description = "Central API Gateway for ERP System services",
        
    });
});

var app = builder.Build();


//app.UseSwagger();
//app.UseSwaggerUI(options =>
//{

//    options.RoutePrefix = string.Empty;
//    options.SwaggerEndpoint("/order/swagger/v1/swagger.json", "Order API v1");
//    options.SwaggerEndpoint("/routing/swagger/v1/swagger.json", "Routing gRPC v1");
//    options.SwaggerEndpoint("/tracking/swagger/v1/swagger.json", "Tracking API v1");
//});


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");


app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new
{
    service = "API Gateway",
    status = "Running",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

app.MapReverseProxy();

await app.RunAsync();

app.Run();
