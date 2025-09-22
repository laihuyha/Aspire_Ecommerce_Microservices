using Microsoft.Extensions.Configuration;

namespace AppHost.Extensions;

public static class BuilderConfigurationExtension
{
    public static void AddAdditionalConfigurationFiles(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddJsonFile("catalog-config.Development.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddJsonFile("basket-config.Development.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddJsonFile("mongo-config.Development.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddJsonFile("postgres-config.Development.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddJsonFile("redis-config.Development.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddJsonFile("rediscommander-config.Development.json", optional: true, reloadOnChange: true);
    }
}
