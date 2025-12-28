using System;

namespace AppHost.Options;

/// <summary>
/// Configuration options for service HTTP/HTTPS ports.
/// Supports both external (host-facing) and internal (container) ports.
/// </summary>
public sealed class ServicePortOptions
{
    /// <summary>
    /// External HTTP port exposed to the host. If null, uses InternalHttpPort.
    /// </summary>
    public int? ExternalHttpPort { get; set; }

    /// <summary>
    /// External HTTPS port exposed to the host. If null, uses InternalHttpsPort.
    /// </summary>
    public int? ExternalHttpsPort { get; set; }

    /// <summary>
    /// Internal HTTP port inside the container.
    /// </summary>
    public int InternalHttpPort { get; set; } = 8080;

    /// <summary>
    /// Internal HTTPS port inside the container.
    /// </summary>
    public int InternalHttpsPort { get; set; } = 8081;

    /// <summary>
    /// Validates the port configuration.
    /// </summary>
    public void Validate(string serviceName)
    {
        if (InternalHttpPort <= 0 || InternalHttpPort > 65535)
        {
            throw new InvalidOperationException($"Invalid internal HTTP port {InternalHttpPort} for service '{serviceName}'.");
        }

        if (InternalHttpsPort <= 0 || InternalHttpsPort > 65535)
        {
            throw new InvalidOperationException($"Invalid internal HTTPS port {InternalHttpsPort} for service '{serviceName}'.");
        }

        if (ExternalHttpPort.HasValue && (ExternalHttpPort.Value <= 0 || ExternalHttpPort.Value > 65535))
        {
            throw new InvalidOperationException($"Invalid external HTTP port {ExternalHttpPort} for service '{serviceName}'.");
        }

        if (ExternalHttpsPort.HasValue && (ExternalHttpsPort.Value <= 0 || ExternalHttpsPort.Value > 65535))
        {
            throw new InvalidOperationException($"Invalid external HTTPS port {ExternalHttpsPort} for service '{serviceName}'.");
        }
    }

    /// <summary>
    /// Creates default port options for a service with specified base port.
    /// HTTP will be basePort, HTTPS will be basePort + 1.
    /// </summary>
    public static ServicePortOptions CreateDefault(int basePort = 8080)
    {
        return new ServicePortOptions
        {
            InternalHttpPort = basePort,
            InternalHttpsPort = basePort + 1
        };
    }

    /// <summary>
    /// Creates port options with explicit external and internal ports.
    /// </summary>
    public static ServicePortOptions Create(int externalHttp, int externalHttps, int internalHttp = 8080, int internalHttps = 8081)
    {
        return new ServicePortOptions
        {
            ExternalHttpPort = externalHttp,
            ExternalHttpsPort = externalHttps,
            InternalHttpPort = internalHttp,
            InternalHttpsPort = internalHttps
        };
    }
}
