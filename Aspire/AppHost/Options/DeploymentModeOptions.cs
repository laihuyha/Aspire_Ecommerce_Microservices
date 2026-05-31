using System;

namespace AppHost.Options;

/// <summary>
/// Options for configuring the deployment workflow mode.
/// </summary>
public class DeploymentModeOptions
{
    /// <summary>
    /// Deployment mode: "direct" for integrated deployment (aspire deploy),
    /// "artifacts" for two-step workflow (aspire publish + docker compose).
    /// Default is "direct".
    /// </summary>
    public string Mode { get; set; } = "direct";

    /// <summary>
    /// Description of the deployment mode configuration.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Checks if artifacts workflow is enabled.
    /// </summary>
    public bool IsArtifactsMode => Mode?.Equals("artifacts", StringComparison.OrdinalIgnoreCase) ?? false;

    /// <summary>
    /// Checks if direct deployment workflow is enabled.
    /// </summary>
    public bool IsDirectMode => Mode?.Equals("direct", StringComparison.OrdinalIgnoreCase) ?? true;
}
