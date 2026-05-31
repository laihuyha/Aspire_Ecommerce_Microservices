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
            // Try multiple detection methods in order of reliability
            string bashPath = FindGitBashFromPath()
                              ?? FindGitBashFromRegistry()
                              ?? FindGitBashFromEnvironment();

            if (!string.IsNullOrEmpty(bashPath))
            {
                return bashPath;
            }

            // No Git Bash found - throw helpful error instead of falling back to WSL
            throw new InvalidOperationException(
                "Git Bash not found. Please install Git for Windows from https://git-scm.com/download/win " +
                "or manually run the certificate script: bash tools/generate-aspire-cert.sh");
        }
        else
        {
            return "/bin/bash";
        }
    }

    private static string FindGitBashFromPath()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "git",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                // git.exe is usually in cmd folder, bash.exe is in bin folder
                string gitPath = output.Split('\n')[0].Trim();
                string gitDir = Path.GetDirectoryName(Path.GetDirectoryName(gitPath));
                if (gitDir != null)
                {
                    string bashPath = Path.Combine(gitDir, "bin", "bash.exe");
                    if (File.Exists(bashPath))
                    {
                        return bashPath;
                    }
                }
            }
        }
        catch
        {
            // Ignore errors when searching PATH
        }

        return null;
    }

    private static string FindGitBashFromRegistry()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return null;
        }

        try
        {
            // Try to find Git installation from Windows Registry
            string[] registryPaths =
            {
                @"SOFTWARE\GitForWindows",
                @"SOFTWARE\WOW6432Node\GitForWindows"
            };

            foreach (string regPath in registryPaths)
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath);
                if (key?.GetValue("InstallPath") is string installPath)
                {
                    string bashPath = Path.Combine(installPath, "bin", "bash.exe");
                    if (File.Exists(bashPath))
                    {
                        return bashPath;
                    }
                }
            }
        }
        catch
        {
            // Registry access may fail, ignore
        }

        return null;
    }

    private static string FindGitBashFromEnvironment()
    {
        // Check common environment variables
        string[] envVars = { "GIT_HOME", "GIT_DIR", "ProgramFiles", "ProgramFiles(x86)", "LocalAppData" };

        foreach (string envVar in envVars)
        {
            string envValue = Environment.GetEnvironmentVariable(envVar);
            if (string.IsNullOrEmpty(envValue))
            {
                continue;
            }

            // For GIT_HOME/GIT_DIR, bash is directly in bin subfolder
            // For ProgramFiles, need to add Git folder
            string[] possiblePaths = envVar.StartsWith("GIT", StringComparison.OrdinalIgnoreCase)
                ? new[] { Path.Combine(envValue, "bin", "bash.exe") }
                : new[]
                {
                    Path.Combine(envValue, "Git", "bin", "bash.exe"),
                    Path.Combine(envValue, "Programs", "Git", "bin", "bash.exe")
                };

            foreach (string bashPath in possiblePaths)
            {
                if (File.Exists(bashPath))
                {
                    return bashPath;
                }
            }
        }

        return null;
    }

    private static string GetShellArguments(string scriptPath)
    {
        // On Windows with Git Bash, just pass the script path
        // On Unix, same approach
        return $"\"{scriptPath}\"";
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
