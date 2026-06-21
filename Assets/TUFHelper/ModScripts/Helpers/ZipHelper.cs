using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using TUFHelper;

public static class ZipExtractor
{
    public static void Unzip(string zipFilePath, string extractFolderPath)
    {
        // Normalize extract path
        if (!extractFolderPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            extractFolderPath += Path.DirectorySeparatorChar;
        }

        string tufhelperPath = Main.ModEntry.Path;
        string sevenZipPath;

        // Determine platform-specific 7-Zip binary path
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            sevenZipPath = Path.Combine(tufhelperPath, "win", "7z.exe");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Separate macOS binary pathway
            sevenZipPath = Path.Combine(tufhelperPath, "mac", "7zz");
            EnsureUnixExecutable(sevenZipPath);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Separate Linux binary pathway
            sevenZipPath = Path.Combine(tufhelperPath, "linux", "7zz");
            EnsureUnixExecutable(sevenZipPath);
        }
        else
        {
            throw new PlatformNotSupportedException("The operating system detected is not supported.");
        }

        // Setup the extraction process
        ProcessStartInfo processStartInfo = new()
        {
            FileName = sevenZipPath,
            Arguments = $"x \"{zipFilePath}\" -o\"{extractFolderPath}\" -y",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            using Process process = Process.Start(processStartInfo);
            if (process == null)
            {
                Main.Logger.Error($"Failed to start the 7-Zip process at {sevenZipPath}");
                return;
            }

            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Main.Logger.Error($"7-Zip extraction failed with exit code {process.ExitCode}:\n{errors}");
            }
        }
        catch (Exception ex)
        {
            Main.Logger.Error($"An unexpected error occurred during extraction: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifies the presence of a Unix binary and ensures permissions are executable (+x).
    /// </summary>
    private static void EnsureUnixExecutable(string binaryPath)
    {
        if (!File.Exists(binaryPath))
        {
            throw new FileNotFoundException($"7zz binary not found at target directory: {binaryPath}");
        }

        try
        {
            using Process chmodProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "/bin/chmod",
                Arguments = $"+x \"{binaryPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            chmodProcess?.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to execute permission fix (chmod +x) on 7zz: {ex.Message}");
        }
    }
}