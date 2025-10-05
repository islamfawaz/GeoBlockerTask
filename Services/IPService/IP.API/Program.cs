using BuildingBlocks.Messaging.Mass_Transiet;
//using BuildingBlocks.Web.ExceptionHandling;
using IP.API.Providers;
using IP.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMessageBroker(builder.Configuration);
builder.Services.AddHttpClient<IIpGeoProvider, IpApiProvider>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseGlobalExceptionHandling();

// Add root endpoint redirect to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.MapIpEndpoints();

app.Run();