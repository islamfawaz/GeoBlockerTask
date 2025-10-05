using Log.API.Infrastructure;

namespace Log.API.Endpoints
{
    public static class LogEndpoints
    {
        public static IEndpointRouteBuilder MapLogEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/logs/blocked-attempts", (ILogStore store, int page = 1, int pageSize = 50) =>
            {
                var (items, total) = store.GetPaged(page, pageSize);
                return Results.Ok(new { items, page, pageSize, total });
            });
            return app;
        }
    }
}


