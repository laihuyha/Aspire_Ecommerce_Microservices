namespace AppHost.Configs;

/// <summary>
/// Docker configuration
/// </summary>
public class Docker
{
    public string? Registry { get; set; }
    public string? Tag { get; set; }
    public Network? Network { get; set; }
}

/// <summary>
/// Docker network configuration
/// </summary>
public class Network
{
    public string? Name { get; set; }
    public string? Driver { get; set; }
    public string? Subnet { get; set; }
}
