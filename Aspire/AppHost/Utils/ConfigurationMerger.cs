using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AppHost.Utils;

public static class ConfigurationMerger
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static IConfigurationBuilder MergeServiceConfigurations(
        IConfigurationBuilder builder,
        string servicesPath,
        string[] serviceDirectories)
    {
        var mergedConfigs = new Dictionary<string, Dictionary<string, object>>();

        // Collect configurations from each service
        foreach (string serviceDir in serviceDirectories)
        {
            string servicePath = Path.Combine(servicesPath, serviceDir);
            if (!Directory.Exists(servicePath))
                continue;

            string serviceName = Path.GetFileName(serviceDir);

            // Collect from service's appsettings files
            var serviceConfigs = CollectServiceConfigurations(servicePath, serviceName);
            MergeConfigurations(mergedConfigs, serviceConfigs);
        }

        // Add merged configurations as in-memory collections
        foreach (var kvp in mergedConfigs)
        {
            builder.AddInMemoryCollection(FlattenConfiguration(kvp.Value, kvp.Key));
        }

        return builder;
    }

    private static Dictionary<string, Dictionary<string, object>> CollectServiceConfigurations(
        string servicePath,
        string serviceName)
    {
        var configs = new Dictionary<string, Dictionary<string, object>>();

        // Common config files to check
        string[] configFiles = {
            "appsettings.json",
            "appsettings.Development.json",
            "appsettings.Local.json"
        };

        string apiPath = Path.Combine(servicePath, "API");

        foreach (string configFile in configFiles)
        {
            // Check in service root first, then in API subdirectory
            string[] possiblePaths = {
                Path.Combine(servicePath, configFile),
                Path.Combine(apiPath, configFile)
            };

            foreach (string configPath in possiblePaths)
            {
                if (File.Exists(configPath))
                {
                    try
                    {
                        var configData = LoadConfigurationFile(configPath);
                        if (configData != null)
                        {
                            // Prefix service-specific configs with service name
                            var prefixedConfig = PrefixConfigurationKeys(configData, $"Services:{serviceName}");
                            MergeConfigurations(configs, prefixedConfig);

                            // Also add global configs without prefix (for shared settings)
                            string[] sectionsToPrefix = { "Database", "Cache", "CertificateSetup", "HttpsCertificate" };
                            var globalConfig = new Dictionary<string, Dictionary<string, object>>();
                            foreach (var kvp in configData)
                            {
                                if (!sectionsToPrefix.Any(section => kvp.Key.StartsWith(section, StringComparison.OrdinalIgnoreCase)))
                                {
                                    if (!globalConfig.ContainsKey(kvp.Key))
                                        globalConfig[kvp.Key] = new Dictionary<string, object>();
                                    globalConfig[kvp.Key][kvp.Key] = kvp.Value;
                                }
                            }
                            MergeConfigurations(configs, globalConfig, allowOverride: false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Config] Warning: Failed to load {configPath} for {serviceName}: {ex.Message}");
                    }
                }
            }
        }

        return configs;
    }

    private static Dictionary<string, object> LoadConfigurationFile(string filePath)
    {
        try
        {
            string jsonContent = File.ReadAllText(filePath);
            var configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent, JsonOptions);
            return configDict != null ? FlattenJsonObject(configDict) : new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    private static Dictionary<string, object> FlattenJsonObject(Dictionary<string, object> obj, string prefix = "")
    {
        var result = new Dictionary<string, object>();

        foreach (var kvp in obj)
        {
            string key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}:{kvp.Key}";

            if (kvp.Value is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.Object)
                {
                    var nested = FlattenJsonObject(jsonElement.Deserialize<Dictionary<string, object>>() ?? new(), key);
                    foreach (var nestedKvp in nested)
                    {
                        result[nestedKvp.Key] = nestedKvp.Value;
                    }
                }
                else if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    result[key] = jsonElement.ToString(); // Keep arrays as JSON strings
                }
                else
                {
                    result[key] = jsonElement.ToString();
                }
            }
            else if (kvp.Value is Dictionary<string, object> nestedObj)
            {
                var nested = FlattenJsonObject(nestedObj, key);
                foreach (var nestedKvp in nested)
                {
                    result[nestedKvp.Key] = nestedKvp.Value;
                }
            }
            else
            {
                result[key] = kvp.Value?.ToString() ?? "";
            }
        }

        return result;
    }

    private static Dictionary<string, Dictionary<string, object>> PrefixConfigurationKeys(
        Dictionary<string, object> config,
        string prefix)
    {
        var result = new Dictionary<string, Dictionary<string, object>>();

        foreach (var kvp in config)
        {
            // Only prefix certain configuration sections
            string[] sectionsToPrefix = { "Database", "Cache", "CertificateSetup", "HttpsCertificate" };

            if (sectionsToPrefix.Any(section => kvp.Key.StartsWith(section, StringComparison.OrdinalIgnoreCase)))
            {
                string prefixedKey = $"{prefix}:{kvp.Key}";
                if (!result.ContainsKey(prefixedKey))
                    result[prefixedKey] = new Dictionary<string, object>();

                result[prefixedKey] = new Dictionary<string, object> { [prefixedKey] = kvp.Value };
            }
        }

        return result;
    }

    private static void MergeConfigurations(
        Dictionary<string, Dictionary<string, object>> target,
        Dictionary<string, Dictionary<string, object>> source,
        bool allowOverride = true)
    {
        foreach (var sectionKvp in source)
        {
            if (!target.ContainsKey(sectionKvp.Key))
            {
                target[sectionKvp.Key] = new Dictionary<string, object>();
            }

            foreach (var configKvp in sectionKvp.Value)
            {
                if (allowOverride || !target[sectionKvp.Key].ContainsKey(configKvp.Key))
                {
                    target[sectionKvp.Key][configKvp.Key] = configKvp.Value;
                }
            }
        }
    }

    private static IEnumerable<KeyValuePair<string, string>> FlattenConfiguration(
        Dictionary<string, object> config,
        string sectionName)
    {
        foreach (var kvp in config)
        {
            yield return new KeyValuePair<string, string>(kvp.Key, kvp.Value?.ToString() ?? "");
        }
    }
}
