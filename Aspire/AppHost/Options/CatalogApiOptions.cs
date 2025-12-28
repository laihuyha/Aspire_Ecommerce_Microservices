namespace AppHost.Options;

public class CatalogApiOptions
{
    public int? ExternalHttpPort { get; set; }
    public int? ExternalHttpsPort { get; set; }
    public int InternalHttpPort { get; set; } = 8080;
    public int InternalHttpsPort { get; set; } = 8081;
}
