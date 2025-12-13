using Microsoft.Extensions.Configuration;

namespace AppHost.Extensions;

public static class BuilderConfigurationExtension
{
    public static void AddAdditionalConfigurationFiles(this IConfigurationBuilder configurationBuilder)
    {
        // Load centralized infra config from root directory
        configurationBuilder.AddJsonFile("../../infra-config.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

        // Keep old configs as fallback (optional: true)
        configurationBuilder.AddJsonFile("catalog-config.Development.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddJsonFile("basket-config.Development.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddJsonFile("postgres-config.Development.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddJsonFile("redis-config.Development.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddJsonFile("rediscommander-config.Development.json", optional: true, reloadOnChange: true);
    }
}
