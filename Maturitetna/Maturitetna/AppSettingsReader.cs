using Microsoft.Extensions.Configuration;


namespace Maturitetna;

public class AppSettingsReader
{ 
    private readonly IConfiguration _configuration;

    public AppSettingsReader(string FilePath)
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile(FilePath, optional: false, reloadOnChange: false);
        _configuration = builder.Build();
    }

    public string GetStringValue(string key)
    {
        return _configuration[key];
    }
}