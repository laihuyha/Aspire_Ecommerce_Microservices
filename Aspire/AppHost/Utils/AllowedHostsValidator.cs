using System;
using System.IO;
using System.Text.Json;
using AppHost.Options;

namespace AppHost.Utils
{
    public static class AllowedHostsValidator
    {
        public static void Validate(string serviceName, string serviceRoot, AllowedHostsValidationOptions options = null)
        {
            options ??= new AllowedHostsValidationOptions();

            if (!options.Enabled)
            {
                return; // Skip validation if disabled
            }

            string devConfig = Path.Combine(
                serviceRoot,
                options.ConfigFileDirectory,
                options.ConfigFileName);

            if (!File.Exists(devConfig))
            {
                throw new InvalidOperationException(
                    $"[Aspire] {serviceName}: Missing {options.ConfigFileName}");
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(devConfig));

                if (!doc.RootElement.TryGetProperty("AllowedHosts", out JsonElement hosts) ||
                    hosts.GetString() != options.RequiredAllowedHostsValue)
                {
                    throw new InvalidOperationException(
                        $"[Aspire] {serviceName}: AllowedHosts must be \"{options.RequiredAllowedHostsValue}\" for development");
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"[Aspire] {serviceName}: Invalid JSON in {options.ConfigFileName}",
                    ex);
            }
        }

        public static void ValidateAllServices(string servicesRoot, AllowedHostsValidationOptions options = null)
        {
            options ??= new AllowedHostsValidationOptions();

            if (!options.Enabled)
            {
                return; // Skip validation if disabled
            }

            foreach (string dir in Directory.GetDirectories(servicesRoot))
            {
                string name = Path.GetFileName(dir);
                Validate(name, dir, options);
            }
        }

        // Backward compatibility method
        public static void Validate(string serviceName, string serviceRoot)
        {
            Validate(serviceName, serviceRoot, new AllowedHostsValidationOptions());
        }

        // Backward compatibility method
        public static void ValidateAllServices(string servicesRoot)
        {
            ValidateAllServices(servicesRoot, new AllowedHostsValidationOptions());
        }
    }
}
