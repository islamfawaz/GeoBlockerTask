using BuildingBlocks.Messaging.Mass_Transiet;
//using BuildingBlocks.Web.ExceptionHandling;
using Log.API.Endpoints;
using Log.API.Consumers;
using Log.API.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ILogStore, InMemoryLogStore>();
builder.Services.AddMessageBroker(builder.Configuration, typeof(Program).Assembly);

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

app.MapLogEndpoints();

app.Run();