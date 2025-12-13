namespace AppHost.Configs;

/// <summary>
/// Main infrastructure configuration
/// </summary>
public class InfraConfig
{
    public string? Version { get; set; }
    public string? Environment { get; set; }
    public Services? Services { get; set; }
    public Apis? Apis { get; set; }
    public Docker? Docker { get; set; }
    public Debug? Debug { get; set; }
}
