
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

//add masstransit
//builder.Services.AddMassTransit(assembly);

builder.Services.AddEndpointsApiExplorer();

//Add Swagger
builder.Services.AddSwagger("Erp API", "v1");


var app = builder.Build();

//Logging with Serilog
app.UseSerilogRequestLogging();

app.UseServicestoErpServices();
app.MapCarter();
app.UseHttpsRedirection();

app.MapSwaggerUI("v1");


app.Run();
