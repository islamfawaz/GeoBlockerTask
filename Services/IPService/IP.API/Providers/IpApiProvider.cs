using System.Net;

namespace IP.API.Providers
{
    public sealed class IpApiProvider : IIpGeoProvider
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;

        public IpApiProvider(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _cfg = cfg;
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
                resp.EnsureSuccessStatusCode();

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
                var url = $"https://api.ipgeolocation.io/ipgeo?apiKey={apiKey}&ip={ip}";
                var resp = await _http.GetAsync(url);
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