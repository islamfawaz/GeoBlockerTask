using BuildingBlocks.Messaging.Mass_Transiet;
using BuildingBlocks.Web.ExceptionHandling;
using Country.Application.BackgroundServices;
using Country.Application.Handlers;
using Country.Domain.Repositories;
using Country.Infrastructure.Repositories;

namespace Country.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(GetBlockedCountriesHandler).Assembly);
            });

            builder.Services.AddSingleton<ICountryRepository, CountryRepository>();
            builder.Services.AddHostedService<ExpiredBlockCleanupService>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddMessageBroker(
                builder.Configuration,
                typeof(GetBlockedCountriesHandler).Assembly
            );

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseGlobalExceptionHandling();

            // Add root endpoint redirect to Swagger
            app.MapGet("/", () => Results.Redirect("/swagger"))
                .ExcludeFromDescription();

            app.MapControllers();
            app.Run();
        }
    }
}