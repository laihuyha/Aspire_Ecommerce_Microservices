using System;
using System.IO;
using System.Text.Json;

namespace AppHost.Utils
{
    public static class AllowedHostsValidator
    {
        public static void Validate(string serviceName, string serviceRoot)
        {
            string devConfig = Path.Combine(
                serviceRoot,
                "API",
                "appsettings.Development.json");

            if (!File.Exists(devConfig))
            {
                throw new InvalidOperationException(
                    $"[Aspire] {serviceName}: Missing appsettings.Development.json");
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(devConfig));

                if (!doc.RootElement.TryGetProperty("AllowedHosts", out JsonElement hosts) ||
                    hosts.GetString() != "*")
                {
                    throw new InvalidOperationException(
                        $"[Aspire] {serviceName}: AllowedHosts must be \"*\" for development");
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"[Aspire] {serviceName}: Invalid JSON in appsettings.Development.json",
                    ex);
            }
        }

        public static void ValidateAllServices(string servicesRoot)
        {
            foreach (string dir in Directory.GetDirectories(servicesRoot))
            {
                string name = Path.GetFileName(dir);
                Validate(name, dir);
            }
        }

        internal static void ValidateAllServices(object servicesPath)
        {
            throw new NotImplementedException();
        }
    }
}
