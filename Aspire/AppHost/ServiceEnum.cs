using System.ComponentModel;
using System.Reflection;

namespace AppHost;

public enum Service
{
    [Description("catalog")]
    CatalogServiceKey
}

public static class ServiceExtensions
{
    public static string GetKey(this Service service)
    {
        var field = service.GetType().GetField(service.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? service.ToString();
    }
}
