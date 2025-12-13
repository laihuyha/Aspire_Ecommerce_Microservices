namespace AppHost.Configs;

public class RedisConfig
{
    public string Image { get; set; }
    public int Port { get; set; }
    public string Volume { get; set; }
}

public class RedisCommanderConfig
{
    public string Image { get; set; }
    public string RedisHosts { get; set; }
    public string HttpUser { get; set; }
    public string HttpPassword { get; set; }
    public int Port { get; set; }
    public int TargetPort { get; set; }
}
