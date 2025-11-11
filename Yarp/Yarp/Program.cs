var builder = WebApplication.CreateBuilder(args);


builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();


//app.UseSwagger();
//app.UseSwaggerUI(options =>
//{

//    options.RoutePrefix = string.Empty;
//    options.SwaggerEndpoint("/order/swagger/v1/swagger.json", "Order API v1");
//    options.SwaggerEndpoint("/routing/swagger/v1/swagger.json", "Routing gRPC v1");
//    options.SwaggerEndpoint("/tracking/swagger/v1/swagger.json", "Tracking API v1");
//});


app.MapReverseProxy();


await app.RunAsync();

app.Run();
