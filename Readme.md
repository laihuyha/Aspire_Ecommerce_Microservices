# Aspire Ecommerce Microservices

This repository contains a modular microservices solution for an ecommerce platform, built using .NET Aspire.

## Solution Structure

- **Aspire/**
  - **AppHost/**: Main entry point for hosting and orchestrating microservices.
    - `AppHost.csproj`, `AppHost.cs`: Project and startup logic.
    - `appsettings.json`, `appsettings.Development.json`: Configuration files.
    - `Properties/launchSettings.json`: Launch profiles for development.
  - **ServiceDefaults/**: Shared service configuration and extensions.
    - `Extensions.cs`: Common extension methods.
    - `ServiceDefaults.csproj`: Project file for shared logic.
  - `AppHost.sln`: Solution file for the entire microservices system.
  - `Directory.Build.props`, `Directory.Packages.props`: Centralized build and package management.

## Getting Started

1. **Prerequisites**
   - [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
   - Visual Studio 2022+ or VS Code

2. **Build the Solution**
   ```powershell
   dotnet build Aspire\AppHost.sln
   ```

3. **Run the AppHost**
   ```powershell
   dotnet run --project Aspire\AppHost\AppHost.csproj
   ```

4. **Configuration**
   - Adjust settings in `appsettings.json` as needed for your environment.

## Project Overview

- **AppHost**: Orchestrates and runs all microservices.
- **ServiceDefaults**: Contains shared configuration and extension methods for services.
