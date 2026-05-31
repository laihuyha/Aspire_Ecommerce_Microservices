# HTTPS Certificate Setup Guide

## Overview

This guide describes how to set up production-like HTTPS for .NET Aspire microservices running in Docker containers.

## Quick Setup

### Option 1: Using dotnet dev-certs (Recommended for Development)

```powershell
# Create certs directory
mkdir certs

# Generate and export certificate
dotnet dev-certs https --export-path certs/aspnetapp.pfx --password 'AspireSecure2024!' --trust

# Build to copy certificate to output
dotnet build
```

### Option 2: Using OpenSSL Script

```bash
# Make script executable
chmod +x tools/generate-aspire-cert.sh

# Run script
./tools/generate-aspire-cert.sh
```

## How It Works

### 1. Certificate Configuration in Service Definition

Certificates are automatically configured in `ServiceDefinitionBase.ConfigureForDocker()`:

```csharp
protected IResourceBuilder<ProjectResource> ConfigureForDocker(
    IResourceBuilder<ProjectResource> projectBuilder,
    ServicePortOptions portOptions,
    HttpsCertificateOptions certOptions)
{
    return projectBuilder.PublishAsDockerComposeService((resource, service) =>
    {
        service.Environment["HTTP_PORTS"] = portOptions.InternalHttpPort.ToString();
        service.Environment["HTTPS_PORTS"] = portOptions.InternalHttpsPort.ToString();

        // HTTPS certificate configuration
        service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Path"] = certOptions.CertificatePath;
        service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Password"] = certOptions.CertificatePassword;
        service.Environment["ASPNETCORE_Kestrel__Certificates__Default__AllowInvalid"] = certOptions.AllowInvalid.ToString().ToLowerInvariant();
    });
}
```

### 2. Certificate Copy in .csproj

Each service's `.csproj` includes a target to copy the certificate:

```xml
<!-- Copy HTTPS certificates to output for Docker deployment -->
<ItemGroup>
    <None Include="$(MSBuildThisFileDirectory)..\..\..\certs\aspnetapp.pfx"
          Condition="Exists('$(MSBuildThisFileDirectory)..\..\..\certs\aspnetapp.pfx')"
          CopyToOutputDirectory="PreserveNewest"
          Link="certs\aspnetapp.pfx"/>
</ItemGroup>
```

### 3. Certificate Options Class

```csharp
public sealed class HttpsCertificateOptions
{
    public string CertificatePath { get; set; } = "/app/certs/aspnetapp.pfx";
    public string CertificatePassword { get; set; } = "AspireSecure2024!";
    public bool AllowInvalid { get; set; } = true;
    public bool Enabled { get; set; } = true;

    // Factory methods
    public static HttpsCertificateOptions Development() => new() { AllowInvalid = true };
    public static HttpsCertificateOptions Production(string certPath, string password) => new()
    {
        CertificatePath = certPath,
        CertificatePassword = password,
        AllowInvalid = false
    };
    public static HttpsCertificateOptions Disabled() => new() { Enabled = false };
}
```

## Configuration via appsettings.json

```json
{
  "HttpsCertificate": {
    "CertificatePath": "/app/certs/aspnetapp.pfx",
    "CertificatePassword": "AspireSecure2024!",
    "AllowInvalid": true,
    "Enabled": true
  },
  "CertificateSetup": {
    "Enabled": true,
    "AutoSetup": true,
    "ForceRegenerate": false
  }
}
```

## Deployment Steps

```powershell
# 1. Generate certificate (if not exists)
dotnet dev-certs https --export-path certs/aspnetapp.pfx --password 'AspireSecure2024!' --trust

# 2. Build solution (copies certificate to output)
dotnet build Aspire\AppHost.sln

# 3. Run with Aspire (development)
dotnet run --project Aspire\AppHost\AppHost.csproj

# 4. Deploy to Docker Compose
cd Aspire\AppHost
aspire deploy -o ./manifests
docker compose --env-file ./manifests/.env.Production up -d
```

## Handling Untrusted Certificate Warnings

### Browser Testing

```bash
# Chrome - ignore certificate errors
chrome.exe --ignore-certificate-errors --ignore-ssl-errors https://localhost:6060

# Firefox - click "Advanced" -> "Accept the Risk and Continue"
```

### API Testing

```bash
# curl - skip certificate verification
curl -k https://localhost:6060/api/catalog

# Postman - Settings -> General -> SSL certificate verification -> OFF
```

### Inter-Service Communication

Certificate is configured with `AllowInvalid = true`, so services can call each other without warnings in Docker network.

## Certificate Details (OpenSSL Script)

- **Algorithm**: RSA 4096-bit
- **Signature**: SHA256
- **Validity**: 10 years
- **Format**: PFX (with password)
- **SAN Entries**:
  - `DNS:localhost`, `DNS:*.localhost`
  - `DNS:catalogapi`, `DNS:catalog-api`
  - `DNS:orderapi`, `DNS:order-api`
  - `DNS:basketapi`, `DNS:basket-api`
  - `IP:127.0.0.1`, `IP:::1`

## Troubleshooting

### "Certificate not found" Error

```bash
# Check if certificate exists in build output
ls Services/Catalog/API/bin/Debug/net9.0/certs/

# Check if certificate exists in container
docker exec -it <container-name> ls -la /app/certs/
```

### "Password incorrect" Error

- Ensure password in configuration matches the certificate password
- Default: `AspireSecure2024!`

### Regenerate Certificate

```powershell
# Using dotnet dev-certs
dotnet dev-certs https --clean
dotnet dev-certs https --export-path certs/aspnetapp.pfx --password 'AspireSecure2024!' --trust

# Using script
./tools/generate-aspire-cert.sh  # Choose 'y' to overwrite
```

### Certificate Not Copied to Output

1. Verify the `.csproj` includes the certificate copy target
2. Check the path condition: `Exists('$(MSBuildThisFileDirectory)..\..\..\certs\aspnetapp.pfx')`
3. Rebuild the project: `dotnet build --no-incremental`

## Production Considerations

For production deployments:

1. Use a proper CA-signed certificate
2. Set `AllowInvalid = false` in `HttpsCertificateOptions`
3. Store certificate password in secure vault (Azure Key Vault, etc.)
4. Use volume mounts instead of baked-in certificates for easier rotation
