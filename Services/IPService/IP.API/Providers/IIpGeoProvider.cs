namespace IP.API.Providers
{
    public interface IIpGeoProvider
    {
        Task<(string? CountryCode,string? CountryName)> LookupAsync(string ip);
    }
}


