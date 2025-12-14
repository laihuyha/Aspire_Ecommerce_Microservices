using System;
using System.IO;

namespace AppHost.Utils
{
    public static class PathHelper
    {
        public static string GetServicesPath()
        {
            // Use the current working directory as reference point
            string currentDir = Directory.GetCurrentDirectory();

            // Navigate up from Aspire/AppHost directory to reach the root
            string rootDirectory = Path.GetFullPath(
                Path.Combine(currentDir, "..", "..")
            );

            return Path.Combine(rootDirectory, "Services");
        }
    }
}
