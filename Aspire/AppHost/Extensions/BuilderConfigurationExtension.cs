using Microsoft.Extensions.Configuration;

namespace AppHost.Extensions;

public static class BuilderConfigurationExtension
{
    public static void AddAdditionalConfigurationFiles(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddJsonFile("catalog-config.Development.json", optional: true, reloadOnChange: true);
    }
}
