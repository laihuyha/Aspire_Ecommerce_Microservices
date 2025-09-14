namespace AppHost.Extensions;

public static class CatalogBuilderExtension
{

    public static IResourceBuilder<ProjectResource> ConfigCatalogEnvironment(this IResourceBuilder<ProjectResource> builder)
    {
        var config = builder.ApplicationBuilder.Configuration.GetSection("CatalogApi");

        // builder.WithEnvironment("ASPNETCORE_ENVIRONMENT", config["Environment"])
        // .WithHttpEndpoint(port: 6000, targetPort: int.Parse(config["HttpPort"]), name: "catalog-http")
        // .WithHttpsEndpoint(port: 6060, targetPort: int.Parse(config["HttpsPort"]), name: "catalog-https");

        return builder;
    }
}
