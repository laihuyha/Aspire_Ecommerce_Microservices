namespace AppHost.Options;

public class AllowedHostsValidationOptions
{
    public string ConfigFileName { get; set; } = "appsettings.Development.json";
    public string ConfigFileDirectory { get; set; } = "API";
    public string RequiredAllowedHostsValue { get; set; } = "*";
    public bool Enabled { get; set; } = true;
}
