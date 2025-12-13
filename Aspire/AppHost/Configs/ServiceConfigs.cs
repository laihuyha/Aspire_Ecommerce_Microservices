using System.Collections.Generic;

namespace AppHost.Configs;

/// <summary>
/// Infrastructure services configuration (databases, cache, etc.)
/// </summary>
public class Services
{
    public Dictionary<string, PostgresInst> Postgres { get; set; }
    public RedisSvc Redis { get; set; }
    public RedisCommanderSvc RedisCommander { get; set; }
}

/// <summary>
/// PostgreSQL instance configuration
/// </summary>
public class PostgresInst
{
    public string Image { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string Db { get; set; }
    public int Port { get; set; }
    public int TargetPort { get; set; }
    public string Volume { get; set; }
    public Healthcheck Healthcheck { get; set; }
    public Resources Resources { get; set; }
}

/// <summary>
/// Redis service configuration
/// </summary>
public class RedisSvc
{
    public string Image { get; set; }
    public int Port { get; set; }
    public int TargetPort { get; set; }
    public string Volume { get; set; }
    public string Command { get; set; }
    public Healthcheck Healthcheck { get; set; }
    public Resources Resources { get; set; }
}

/// <summary>
/// Redis Commander configuration
/// </summary>
public class RedisCommanderSvc
{
    public string Image { get; set; }
    public int Port { get; set; }
    public int TargetPort { get; set; }
    public string HttpUser { get; set; }
    public string HttpPassword { get; set; }
    public string RedisHosts { get; set; }
    public Resources Resources { get; set; }
}
