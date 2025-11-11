using Microsoft.EntityFrameworkCore.Migrations;

var builder = WebApplication.CreateBuilder(args);

//Add Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

var assembly = typeof(Program).Assembly;

builder.Services.AddServicestoDataMigration(builder.Configuration);

//Add custom services
builder.Services.AddServicestoDataMigration(builder.Configuration);

//Add Automapper
var autoMapperLicenseKey = builder.Configuration["AutoMapper:LicenseKey"];
builder.Services.AddAutoMapper(cfg => cfg.LicenseKey = autoMapperLicenseKey, assembly);


//Add MediatR
builder.Services.AddMediatRExtension(assembly);



//Add Masstransit
//builder.Services.AddMassTransit(assembly);



var app = builder.Build();
app.UseSerilogRequestLogging();

// Run migration
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Data Migration Service Started");

try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    // File paths
    var clientsFile = Path.Combine("data", "Clients.xlsx");
    var workOrdersFile = Path.Combine("data", "WorkOrders.xlsx");

    // Import Clients
    logger.LogInformation("═══════════════════════════════════════");
    logger.LogInformation("1: Importing Clients");
    logger.LogInformation("═══════════════════════════════════════");

    var clientService = services.GetRequiredService<IClientImportService>();
    var clientResult = await clientService.ImportClientsAsync(clientsFile);

    logger.LogInformation("Client Import Summary:");
    logger.LogInformation("Total Rows: {Total:N0}", clientResult.TotalRows);
    logger.LogInformation("Successful: {Success:N0}", clientResult.SuccessfulRows);
    logger.LogInformation("Failed: {Failed:N0}", clientResult.FailedRows);
    logger.LogInformation("Duration: {Duration:F2}s", clientResult.Duration.TotalSeconds);

    // Import Technicians
    logger.LogInformation("");
    logger.LogInformation("═══════════════════════════════════════");
    logger.LogInformation("2: Importing Technicians");
    logger.LogInformation("═══════════════════════════════════════");

    var techService = services.GetRequiredService<ITechnicianImportService>();
    var techResult = await techService.ImportTechniciansAsync(workOrdersFile);

    logger.LogInformation("Technician Import Summary:");
    logger.LogInformation("Total Rows: {Total:N0}", techResult.TotalRows);
    logger.LogInformation("Successful: {Success:N0}", techResult.SuccessfulRows);
    logger.LogInformation("Failed: {Failed:N0}", techResult.FailedRows);
    logger.LogInformation("Duration: {Duration:F2}s", techResult.Duration.TotalSeconds);

    // Import Work Orders
    logger.LogInformation("");
    logger.LogInformation("═══════════════════════════════════════");
    logger.LogInformation("3: Importing Work Orders");
    logger.LogInformation("═══════════════════════════════════════");

    var workOrderService = services.GetRequiredService<IWorkOrderImportService>();
    var workOrderResult = await workOrderService.ImportWorkOrdersAsync(workOrdersFile);

    logger.LogInformation("Work Order Import Summary:");
    logger.LogInformation("Total Rows: {Total:N0}", workOrderResult.TotalRows);
    logger.LogInformation("Successful: {Success:N0}", workOrderResult.SuccessfulRows);
    logger.LogInformation("Failed: {Failed:N0}", workOrderResult.FailedRows);
    logger.LogInformation("Duration: {Duration:F2}s", workOrderResult.Duration.TotalSeconds);
    logger.LogInformation("Throughput: {Rate:N0} rows/sec",
        workOrderResult.SuccessfulRows / workOrderResult.Duration.TotalSeconds);

    if (workOrderResult.Errors.Any())
    {
        logger.LogWarning("First 10 errors:");
        foreach (var error in workOrderResult.Errors.Take(10))
        {
            logger.LogWarning("   {Error}", error);
        }
    }

    logger.LogInformation("");
    logger.LogInformation("═══════════════════════════════════════");
    logger.LogInformation("MIGRATION COMPLETED SUCCESSFULLY");
    logger.LogInformation("═══════════════════════════════════════");

    var totalDuration = clientResult.Duration + techResult.Duration + workOrderResult.Duration;
    logger.LogInformation("Total Time: {Duration:F2}s", totalDuration.TotalSeconds);
}
catch (Exception ex)
{
    logger.LogError(ex, "Migration failed");
    throw;
}

await app.RunAsync();




