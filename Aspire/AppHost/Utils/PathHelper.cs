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

        public static string GetProjectRootPath()
        {
            // Use the current working directory as reference point (should be Aspire/AppHost when running)
            string currentDir = Directory.GetCurrentDirectory();

            // Navigate up from Aspire/AppHost directory to reach the project root
            string rootDirectory = Path.GetFullPath(
                Path.Combine(currentDir, "..", "..")
            );

            return rootDirectory;
        }

        public static string GetProjectRootPathFromBaseDirectory()
        {
            // Alternative method using AppContext.BaseDirectory
            // This is more reliable when the process working directory might be different
            string baseDir = AppContext.BaseDirectory;

            // Navigate up from Aspire/AppHost/bin/Debug/net8.0 to reach the project root
            string rootDirectory = Path.GetFullPath(
                Path.Combine(baseDir, "..", "..", "..")
            );

            return rootDirectory;
        }
    }
}
