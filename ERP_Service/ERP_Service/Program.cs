
using ERP_Service.Data;
using MassTransit;
using Microsoft.OpenApi;
using SharedKernel.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

//Add Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});


var assembly = typeof(Program).Assembly;

builder.Services.AddServicestoErpService(builder.Configuration);


//Add Automapper
var autoMapperLicenseKey = builder.Configuration["AutoMapper:LicenseKey"];
builder.Services.AddAutoMapper(cfg => cfg.LicenseKey = autoMapperLicenseKey, assembly);


//Add MediatR
builder.Services.AddMediatRExtension(assembly);

//Add carter
builder.Services.AddCarter(assembly);


//Add MassTransit with EF Outbox
builder.Services.AddMassTransit<ERP_Dbcontext>(
    builder.Configuration,
    assembly,
    dbOutboxConfig =>
    {
        dbOutboxConfig.UseSqlServer();
        dbOutboxConfig.UseBusOutbox();
    }
);

builder.Services.AddEndpointsApiExplorer();

//Add Swagger
//builder.Services.AddSwagger("Erp API", "v1");
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ERP Service API",
        Version = "v1",
        Description = "Sample ERP service ",
    });

 });

// Add Health Checks
builder.Services.AddApplicationHealthChecks<ERP_Dbcontext>("ERP Service");



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



var app = builder.Build();



//Logging with Serilog
app.UseSerilogRequestLogging();

app.UseServicestoErpServices();
app.MapCarter();
app.UseHttpsRedirection();
app.UseApplicationHealthChecks();
app.UseCors("AllowAll");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP Service API v1");
        c.RoutePrefix = "swagger";
    });
}


app.Run();
