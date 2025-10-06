using BuildingBlocks.Messaging.Mass_Transiet;
using IP.API.Endpoints;
using IP.API.Providers;
using MassTransit;
using IP.API.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HTTP client for IP lookup
builder.Services.AddHttpClient<IIpGeoProvider, IpApiProvider>();

// Memory cache to store blocked countries
builder.Services.AddMemoryCache();

// Add MassTransit and register consumers
builder.Services.AddMassTransit(config =>
{
    config.SetKebabCaseEndpointNameFormatter();

    // Register consumers
    config.AddConsumer<CountryBlockedConsumer>();
    config.AddConsumer<CountryUnblockedConsumer>();

    config.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(new Uri(builder.Configuration["MessageBroker:Host"]!), host =>
        {
            host.Username(builder.Configuration["MessageBroker:Username"]!);
            host.Password(builder.Configuration["MessageBroker:Password"]!);
        });

        // Automatically configure endpoints for registered consumers
        configurator.ConfigureEndpoints(context);
    });
});

// Build app
var app = builder.Build();

// Development middleware to replace localhost IP with test IP
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var originalIp = context.Connection.RemoteIpAddress?.ToString();

        if (originalIp == "::1" || originalIp == "127.0.0.1" || originalIp?.StartsWith("::ffff:127.0.0.1") == true)
        {
            var testIp = context.Request.Query["testIp"].FirstOrDefault() ?? "8.8.8.8";
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(testIp);
            Console.WriteLine($"[DEV] Replaced localhost IP with test IP: {testIp}");
        }

        await next();
    });

    app.UseSwagger();
    app.UseSwaggerUI();
}

// Root redirect to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

// Map IP endpoints
app.MapIpEndpoints();

// Run app
app.Run();
