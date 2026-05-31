using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace AppHost.Utils;

/// <summary>
/// Provides a cached merged configuration for AppHost.
/// Loads all AppHost config files (appsettings.json, infrastructure.json, validation.json)
/// and makes them available throughout the application.
/// </summary>
public static class AppHostConfiguration
{
    private static IConfiguration _cachedConfiguration;
    private static readonly object _lock = new();

    /// <summary>
    /// Gets the merged configuration including all AppHost config files.
    /// Configuration is built once and cached for subsequent calls.
    /// </summary>
    /// <param name="baseConfiguration">Optional base configuration to merge with (e.g., builder.Configuration)</param>
    /// <returns>Merged configuration containing all AppHost settings</returns>
    public static IConfiguration GetMergedConfiguration(IConfiguration baseConfiguration = null)
    {
        if (_cachedConfiguration != null)
        {
            return _cachedConfiguration;
        }

        lock (_lock)
        {
            if (_cachedConfiguration != null)
            {
                return _cachedConfiguration;
            }

            _cachedConfiguration = BuildMergedConfiguration(baseConfiguration);
            return _cachedConfiguration;
        }
    }

    /// <summary>
    /// Forces a rebuild of the cached configuration.
    /// Useful for testing or when config files change at runtime.
    /// </summary>
    public static void ResetCache()
    {
        lock (_lock)
        {
            _cachedConfiguration = null;
        }
    }

    private static IConfiguration BuildMergedConfiguration(IConfiguration baseConfiguration)
    {
        string baseDirectory = AppContext.BaseDirectory;

        var configBuilder = new ConfigurationBuilder();

        // Add base configuration if provided
        if (baseConfiguration != null)
        {
            configBuilder.AddConfiguration(baseConfiguration);
        }

        // Add all AppHost config files in order (later files override earlier ones)
        string[] configFiles =
        {
            "appsettings.json",
            "infrastructure.json",
            "validation.json",
            "appsettings.Development.json",
            "appsettings.Production.json"
        };

        foreach (string configFile in configFiles)
        {
            string configPath = Path.Combine(baseDirectory, configFile);
            if (File.Exists(configPath))
            {
                configBuilder.AddJsonFile(configPath, optional: true, reloadOnChange: false);
            }
        }

        // Also check environment variables
        configBuilder.AddEnvironmentVariables();

        return configBuilder.Build();
    }
}
