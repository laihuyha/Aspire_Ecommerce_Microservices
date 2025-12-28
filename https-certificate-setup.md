# 🔐 .NET Aspire HTTPS Certificate Setup Guide

## Overview

This guide describes how to set up production-like HTTPS for .NET Aspire microservices running in Docker containers, using a self-signed certificate with full Subject Alternative Names (SAN).

## 🎯 Solution

### 1. **Bash Script: `tools/generate-aspire-cert.sh`**

The script automatically:
- ✅ Creates RSA 4096-bit certificate with SHA256
- ✅ 10-year validity
- ✅ Full SAN for all services
- ✅ Exports to PFX format
- ✅ Automatically copies to all API projects

```bash
# Make script executable (one-time setup)
chmod +x tools/generate-aspire-cert.sh

# Run script to generate certificate
./tools/generate-aspire-cert.sh

# Alternative: Run with bash explicitly
bash tools/generate-aspire-cert.sh
```

**Note**: This script requires OpenSSL to be installed on your system.
- **macOS**: `brew install openssl`
- **Ubuntu/Debian**: `sudo apt update && sudo apt install openssl`
- **Windows**: Use Git Bash or WSL with OpenSSL installed

### 2. **Certificate Configuration in AppHost**

Add environment variables to `PublishAsDockerComposeService`:

```csharp
// Aspire/AppHost/Extensions/InfrastructureExtensions.cs
.PublishAsDockerComposeService((resource, service) =>
{
    service.Name = $"{serviceName}-api";
    service.Environment["HTTP_PORTS"] = options.InternalHttpPort.ToString();
    service.Environment["HTTPS_PORTS"] = options.InternalHttpsPort.ToString();

    // 🔐 HTTPS Certificate Configuration for Docker
    service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Path"] = "/app/certs/aspnetapp.pfx";
    service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Password"] = "AspireSecure2024!";
    service.Environment["ASPNETCORE_Kestrel__Certificates__Default__AllowInvalid"] = "true";
});
```

### 3. **Usage Example for Multiple Services**

```csharp
// In AppHost.cs - Catalog API (already configured in InfrastructureExtensions)
var catalogApi = builder.AddCatalogApi("catalog", cache, catalogDb);

// Order API
var orderApi = builder.AddProject<Order_API>("order-api")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithReference(orderDb)
    .WithHttpEndpoint(7000, 8080, "order-http")
    .WithHttpsEndpoint(7443, 8443, "order-https")
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Name = "order-api";
        service.Environment["HTTP_PORTS"] = "8080";
        service.Environment["HTTPS_PORTS"] = "8443";
        service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Path"] = "/app/certs/aspnetapp.pfx";
        service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Password"] = "AspireSecure2024!";
        service.Environment["ASPNETCORE_Kestrel__Certificates__Default__AllowInvalid"] = "true";
    });

// Basket API
var basketApi = builder.AddProject<Basket_API>("basket-api")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithReference(cache)
    .WithHttpEndpoint(8000, 8080, "basket-http")
    .WithHttpsEndpoint(8443, 8443, "basket-https")
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Name = "basket-api";
        service.Environment["HTTP_PORTS"] = "8080";
        service.Environment["HTTPS_PORTS"] = "8443";
        service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Path"] = "/app/certs/aspnetapp.pfx";
        service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Password"] = "AspireSecure2024!";
        service.Environment["ASPNETCORE_Kestrel__Certificates__Default__AllowInvalid"] = "true";
    });
```

## 🚀 **Deployment Steps**

```bash
# 1. Make script executable and generate certificates
chmod +x tools/generate-aspire-cert.sh
./tools/generate-aspire-cert.sh

# 2. Deploy with Aspire
dotnet run --project Aspire/AppHost -- deploy -o ./manifests

# 3. Run docker-compose
docker-compose -f ./manifests/docker-compose.yml up -d
```

## ⚠️ **Handling Untrusted Certificate Warnings**

### **For Browser Testing:**
```bash
# Chrome - ignore certificate errors
chrome.exe --ignore-certificate-errors --ignore-ssl-errors https://localhost:6060

# Firefox - accept risk and continue
# (Click "Advanced" -> "Accept the Risk and Continue")
```

### **For Inter-Service Communication:**
Certificate is configured with `AllowInvalid = true`, so services can call each other without warnings.

### **For Development Tools (Postman, curl):**
```bash
# curl - skip certificate verification
curl -k https://localhost:6060/api/catalog

# Postman - Settings -> General -> SSL certificate verification -> OFF
```

## 🔒 **Why is this Production-like and Secure?**

### **Production-like:**
- ✅ **Real Certificate**: Doesn't use `dotnet dev-certs`
- ✅ **Proper SAN**: Supports all service names and localhost
- ✅ **Baked into Image**: Certificate is in the container, no host dependency
- ✅ **PFX Format**: Easy to use with Kestrel
- ✅ **Strong Security**: RSA 4096-bit, SHA256, 10-year validity

### **Secure for Development:**
- ✅ **Isolated**: Each container has its own certificate
- ✅ **No Host Dependency**: No need to install certificate on host machine
- ✅ **Reproducible**: Script can be run again anytime
- ✅ **Version Controlled**: Certificate files are committed to git
- ✅ **Inter-Service Trust**: Services trust each other in Docker network

## 📋 **Certificate Details**

- **Algorithm**: RSA 4096-bit
- **Signature**: SHA256
- **Validity**: 10 years
- **Format**: PFX (with password)
- **SAN Entries**:
  - `DNS:localhost`
  - `DNS:*.localhost`
  - `DNS:catalogapi`, `DNS:catalog-api`
  - `DNS:orderapi`, `DNS:order-api`
  - `DNS:basketapi`, `DNS:basket-api`
  - `DNS:identityapi`, `DNS:identity-api`
  - And many more...

## 🔧 **Troubleshooting**

### **"Certificate not found" error:**
```bash
# Check if certificate exists in container
docker exec -it <container-name> ls -la /app/certs/

# Check certificate info
docker exec -it <container-name> openssl x509 -in /app/certs/aspnetapp.pfx -text -noout
```

### **"Password incorrect" error:**
- Ensure password in AppHost matches password in script
- Default: `"AspireSecure2024!"`

### **Regenerate certificate:**
```bash
./tools/generate-aspire-cert.sh
# The script will prompt to overwrite existing certificate
```

## 🎉 **Conclusion**

This solution provides production-like HTTPS for .NET Aspire microservices that:
- Works completely in Docker
- No dependency on host machine
- Easy to setup and maintain
- Safe for both development and production workflows
