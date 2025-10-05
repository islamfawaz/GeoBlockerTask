using BuildingBlocks.Messaging.Contracts;
using IP.API.Providers;
using MassTransit;

namespace IP.API.Endpoints
{
    public static class IpEndpoints
    {
        public static IEndpointRouteBuilder MapIpEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/ip");

            group.MapGet("/lookup", async (HttpContext ctx, string? ipAddress, IIpGeoProvider provider) =>
            {
                var ip = string.IsNullOrWhiteSpace(ipAddress) ? ctx.Connection.RemoteIpAddress?.ToString() ?? string.Empty : ipAddress;
                if (string.IsNullOrWhiteSpace(ip)) return Results.BadRequest(new { error = "Cannot determine caller IP" });
                var geo = await provider.LookupAsync(ip);
                return Results.Ok(new { ip, geo.CountryCode });
            });

            group.MapGet("/check-block", async (HttpContext ctx, IIpGeoProvider provider, IHttpClientFactory http, IPublishEndpoint publisher, IConfiguration cfg) =>
            {
                var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(ip)) return Results.BadRequest(new { error = "Cannot determine caller IP" });

                var geo = await provider.LookupAsync(ip);
                var countryCode = geo.CountryCode ?? string.Empty;

                var client = http.CreateClient();
                client.BaseAddress = new Uri(cfg["Downstreams:Country"] ?? "http://localhost:5001/");
                var resp = await client.GetAsync($"api/countries/{countryCode}");
                var isBlocked = resp.IsSuccessStatusCode;

                await publisher.Publish(new BlockedIpAttemptIntegrationEvent(ip, countryCode, DateTime.UtcNow, ctx.Request.Headers.UserAgent.ToString(), isBlocked));
                return Results.Ok(new { ip, countryCode, isBlocked });
            });

            return app;
        }
    }
}


