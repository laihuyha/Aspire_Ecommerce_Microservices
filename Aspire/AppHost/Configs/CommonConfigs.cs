namespace AppHost.Configs;

/// <summary>
/// Health check configuration
/// </summary>
public class Healthcheck
{
    public string Interval { get; set; }
    public string Timeout { get; set; }
    public int Retries { get; set; }
    public string StartPeriod { get; set; }
    public string Test { get; set; }
}

/// <summary>
/// Resource limits configuration
/// </summary>
public class Resources
{
    public ResourceLimits Limits { get; set; }
    public ResourceLimits Reservations { get; set; }
}

/// <summary>
/// Resource limits (memory, CPU, etc.)
/// </summary>
public class ResourceLimits
{
    public string Memory { get; set; }
    public string Cpus { get; set; }
}

/// <summary>
/// Debug configuration
/// </summary>
public class Debug
{
    public bool ForceRestart { get; set; }
}
