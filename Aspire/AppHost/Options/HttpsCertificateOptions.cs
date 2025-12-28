using System;

namespace AppHost.Options;

/// <summary>
/// Configuration options for HTTPS certificates in Docker deployments.
/// </summary>
public sealed class HttpsCertificateOptions
{
    /// <summary>
    /// Path to the certificate file inside the container.
    /// Default: /app/certs/aspnetapp.pfx
    /// </summary>
    public string CertificatePath { get; set; } = "/app/certs/aspnetapp.pfx";

    /// <summary>
    /// Password for the PFX certificate file.
    /// </summary>
    public string CertificatePassword { get; set; } = "AspireSecure2024!";

    /// <summary>
    /// Whether to allow invalid/self-signed certificates.
    /// Set to true for development, false for production.
    /// </summary>
    public bool AllowInvalid { get; set; } = true;

    /// <summary>
    /// Whether HTTPS is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Validates the certificate configuration.
    /// </summary>
    public void Validate()
    {
        if (!Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(CertificatePath))
        {
            throw new InvalidOperationException("Certificate path is required when HTTPS is enabled.");
        }

        if (string.IsNullOrWhiteSpace(CertificatePassword))
        {
            throw new InvalidOperationException("Certificate password is required when HTTPS is enabled.");
        }
    }

    /// <summary>
    /// Creates options for development with self-signed certificate.
    /// </summary>
    public static HttpsCertificateOptions Development() => new()
    {
        CertificatePath = "/app/certs/aspnetapp.pfx",
        CertificatePassword = "AspireSecure2024!",
        AllowInvalid = true,
        Enabled = true
    };

    /// <summary>
    /// Creates options for production with proper certificate.
    /// </summary>
    public static HttpsCertificateOptions Production(string certPath, string certPassword) => new()
    {
        CertificatePath = certPath,
        CertificatePassword = certPassword,
        AllowInvalid = false,
        Enabled = true
    };

    /// <summary>
    /// Creates options with HTTPS disabled (HTTP only).
    /// </summary>
    public static HttpsCertificateOptions Disabled() => new()
    {
        Enabled = false
    };
}
