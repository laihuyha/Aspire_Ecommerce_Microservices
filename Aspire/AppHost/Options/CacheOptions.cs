using System;

namespace AppHost.Options;

public class CacheOptions
{
    public string Image { get; set; } = "redis:7-alpine";
    public string VolumeName { get; set; } = "redis_data";
    public int PersistenceIntervalMinutes { get; set; } = 5;
    public int PersistenceKeys { get; set; } = 100;
    public string MaxMemory { get; set; } = "128mb";
    public string MaxMemoryPolicy { get; set; } = "allkeys-lru";

    public TimeSpan PersistenceInterval => TimeSpan.FromMinutes(PersistenceIntervalMinutes);
}
