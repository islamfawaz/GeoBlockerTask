using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace IP.API.Providers
{
    public sealed class IpApiProvider : IIpGeoProvider
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;
        private readonly IMemoryCache _cache;

        public IpApiProvider(HttpClient http, IConfiguration cfg , IMemoryCache cache)
        {
            _http = http;
            _cfg = cfg;
            _cache = cache;
        }

        public async Task<(string? CountryCode, string? CountryName)> LookupAsync(string ip)
        {
            if (!IPAddress.TryParse(ip, out _))
                throw new ArgumentException("Invalid IP address");

            var apiKey = _cfg["GeoIp:ApiKey"];
            var provider = _cfg["GeoIp:Provider"] ?? "ipapi";

            if (string.Equals(provider, "ipapi", StringComparison.OrdinalIgnoreCase))
            {
                var url = $"https://ipapi.co/{ip}/json/";
                var resp = await _http.GetAsync(url);

                // CHECK STATUS BEFORE EnsureSuccessStatusCode
                if (resp.StatusCode == System.Net.HttpStatusCode.Locked ||
                    resp.StatusCode == (System.Net.HttpStatusCode)429)
                {
                    // Return default values or throw a custom exception
                    throw new InvalidOperationException(
                        "IP geolocation API rate limit exceeded. Please try again later or use a different provider.");
                }

                resp.EnsureSuccessStatusCode(); // This line won't throw for 423 anymore

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await System.Text.Json.JsonDocument.ParseAsync(stream);

                var countryCode = doc.RootElement.TryGetProperty("country", out var ccProp)
                    ? ccProp.GetString()
                    : null;
                var countryName = doc.RootElement.TryGetProperty("country_name", out var cnProp)
                    ? cnProp.GetString()
                    : null;

                return (countryCode, countryName);
            }
            else
            {
                // ipgeolocation.io code
                var url = $"https://api.ipgeolocation.io/ipgeo?apiKey={apiKey}&ip={ip}";
                var resp = await _http.GetAsync(url);

                // CHECK STATUS BEFORE EnsureSuccessStatusCode
                if (resp.StatusCode == System.Net.HttpStatusCode.Locked ||
                    resp.StatusCode == (System.Net.HttpStatusCode)429)
                {
                    throw new InvalidOperationException(
                        "IP geolocation API rate limit exceeded. Please try again later.");
                }

                resp.EnsureSuccessStatusCode();

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await System.Text.Json.JsonDocument.ParseAsync(stream);

                var countryCode = doc.RootElement.TryGetProperty("country_code2", out var ccProp)
                    ? ccProp.GetString()
                    : null;
                var countryName = doc.RootElement.TryGetProperty("country_name", out var cnProp)
                    ? cnProp.GetString()
                    : null;

                return (countryCode, countryName);
            }
        }
    }
}