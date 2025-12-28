using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using AppHost.Options;
using AppHost.PathConstants;

namespace AppHost.Utils;

public static class SelfSignCertificateSetup
{
    public static void SetupCertificates(string projectRoot, bool forceRegenerate = false)
    {
        try
        {
            string scriptPath = Path.Combine(projectRoot, Constants.CertificateScript);
            string certsDir = Path.Combine(projectRoot, Constants.CertificateDirectory);
            string certFile = Path.Combine(certsDir, Constants.CertificateFile);

            // Check if script exists
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException(
                    $"Certificate generation script not found: {scriptPath}",
                    scriptPath);
            }

            // Check if certificates already exist and not forcing regeneration
            if (!forceRegenerate && File.Exists(certFile))
            {
                Console.WriteLine($"[Aspire] Certificate already exists at: {certFile}");
                return;
            }

            Console.WriteLine($"[Aspire] Setting up HTTPS certificates...");

            // Make script executable on Unix systems
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                MakeScriptExecutable(scriptPath);
            }

            // Run certificate generation script
            RunCertificateScript(scriptPath, projectRoot);

            // Verify certificate was created
            if (!File.Exists(certFile))
            {
                throw new InvalidOperationException(
                    $"Certificate generation failed. Expected file not found: {certFile}");
            }

            // Note: Certificate will be copied to application output directory via MSBuild targets in .csproj
            Console.WriteLine($"[Aspire] 📝 Certificate will be included in Docker image via MSBuild copy targets");

            Console.WriteLine($"[Aspire] ✅ Certificate setup completed: {certFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Aspire] ❌ Certificate setup failed: {ex.Message}");
            throw new InvalidOperationException("Failed to setup HTTPS certificates", ex);
        }
    }

    private static void MakeScriptExecutable(string scriptPath)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{scriptPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Failed to make script executable: chmod exited with code {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Aspire] Warning: Could not make script executable: {ex.Message}");
        }
    }

    private static void RunCertificateScript(string scriptPath, string workingDirectory)
    {
        string shell = GetShellCommand();
        string arguments = GetShellArguments(scriptPath);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine($"[Certificate] {e.Data}");
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine($"[Certificate Error] {e.Data}");
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Certificate script exited with code {process.ExitCode}");
        }
    }

    private static string GetShellCommand()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Check if Git Bash is available, otherwise use cmd
            string gitBashPath = @"C:\Program Files\Git\bin\bash.exe";
            if (File.Exists(gitBashPath))
            {
                return gitBashPath;
            }

            // Fall back to cmd
            return "cmd.exe";
        }
        else
        {
            return "/bin/bash";
        }
    }

    private static string GetShellArguments(string scriptPath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string shell = GetShellCommand();
            if (shell.EndsWith("bash.exe", StringComparison.OrdinalIgnoreCase))
            {
                return $"\"{scriptPath}\"";
            }
            else
            {
                return $"/c bash \"{scriptPath}\"";
            }
        }
        else
        {
            return $"\"{scriptPath}\"";
        }
    }

    public static bool ShouldSetupCertificates(CertificateSetupOptions options)
    {
        // Check options for certificate setup trigger
        return options.Enabled || options.AutoSetup;
    }

    public static void SetupIfEnabled(CertificateSetupOptions options, string projectRoot)
    {
        if (ShouldSetupCertificates(options))
        {
            SetupCertificates(projectRoot, options.ForceRegenerate);
        }
        else
        {
            Console.WriteLine("[Aspire] Certificate auto-setup is disabled. Run manually if needed.");
        }
    }
}
