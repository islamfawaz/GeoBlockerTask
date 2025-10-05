var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient("country", c => c.BaseAddress = new Uri(builder.Configuration["Downstreams:Country"] ?? "http://localhost:5001/"));
builder.Services.AddHttpClient("ip", c => c.BaseAddress = new Uri(builder.Configuration["Downstreams:IP"] ?? "http://localhost:5002/"));
builder.Services.AddHttpClient("log", c => c.BaseAddress = new Uri(builder.Configuration["Downstreams:Log"] ?? "http://localhost:5003/"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Proxy endpoints
app.MapPost("/api/countries/block", async (HttpRequest req, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("country");
    using var content = new StreamContent(req.Body);
    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
    var resp = await client.PostAsync("api/countries/block", content);
    return Results.Content(await resp.Content.ReadAsStringAsync(), resp.Content.Headers.ContentType?.ToString(), null, (int)resp.StatusCode);
});

app.MapPost("/api/countries/temporal-block", async (HttpRequest req, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("country");
    using var content = new StreamContent(req.Body);
    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
    var resp = await client.PostAsync("api/countries/temporal-block", content);
    return Results.Content(await resp.Content.ReadAsStringAsync(), resp.Content.Headers.ContentType?.ToString(),null, (int)resp.StatusCode);
});

app.MapDelete("/api/countries/block/{countryCode}", async (string countryCode, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("country");
    var resp = await client.DeleteAsync($"api/countries/block/{countryCode}");
    return Results.Content(await resp.Content.ReadAsStringAsync(), resp.Content.Headers.ContentType?.ToString(),null, (int)resp.StatusCode);
});

app.MapGet("/api/ip/lookup", async (HttpRequest req, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("ip");
    var q = req.QueryString.HasValue ? req.QueryString.Value : "";
    var resp = await client.GetAsync($"api/ip/lookup{q}");
    return Results.Content(await resp.Content.ReadAsStringAsync(), resp.Content.Headers.ContentType?.ToString(),null, (int)resp.StatusCode);
});

app.MapGet("/api/ip/check-block", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("ip");
    var resp = await client.GetAsync($"api/ip/check-block");
    return Results.Content(await resp.Content.ReadAsStringAsync(), resp.Content.Headers.ContentType?.ToString(),null, (int)resp.StatusCode);
});

app.MapGet("/api/logs/blocked-attempts", async (int page, int pageSize, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("log");
    var resp = await client.GetAsync($"api/logs/blocked-attempts?page={page}&pageSize={pageSize}");
    return Results.Content(await resp.Content.ReadAsStringAsync(), resp.Content.Headers.ContentType?.ToString(), null, (int)resp.StatusCode);
});

app.Run();


