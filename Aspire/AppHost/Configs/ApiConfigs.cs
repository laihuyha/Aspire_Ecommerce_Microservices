namespace AppHost.Configs;

/// <summary>
/// API services configuration
/// </summary>
public class Apis
{
    public CatalogApi Catalog { get; set; }
}

/// <summary>
/// Catalog API configuration
/// </summary>
public class CatalogApi
{
    public string Image { get; set; }
    public int HttpPort { get; set; }
    public int HttpsPort { get; set; }
    public int TargetHttpPort { get; set; }
    public int TargetHttpsPort { get; set; }
    public string Environment { get; set; }
    public Healthcheck Healthcheck { get; set; }
    public Resources Resources { get; set; }
}
