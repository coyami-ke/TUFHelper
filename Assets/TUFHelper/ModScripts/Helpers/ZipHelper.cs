using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace TUFHelper.Utils
{
    public static class ZipHelper
    {
        public static void Unzip(string zipFilePath, string extractFolderPath)
        {
            // Normalize extract path
            if (!extractFolderPath.EndsWith(Path.DirectorySeparatorChar))
            {
                extractFolderPath += Path.DirectorySeparatorChar;
            }

            string tufhelperPath = Main.FindTUFHelperPath();

            // Determine platform-specific 7-Zip binary
            string sevenZipPath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                sevenZipPath = Path.Combine(tufhelperPath, "7zip", "7z.exe");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                sevenZipPath = Path.Combine(tufhelperPath, "7zip", "7zz");

                if (!File.Exists(sevenZipPath))
                    throw new FileNotFoundException($"7zz binary not found at {sevenZipPath}");

                // Ensure executable
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "/bin/chmod",
                        Arguments = $"+x \"{sevenZipPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    })?.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to chmod 7zz: {ex.Message}");
                }
            }
            else
            {
                throw new PlatformNotSupportedException("Only Windows and Linux platforms are supported.");
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

            using Process process = Process.Start(processStartInfo);
            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                //throw new Exception($"7-Zip extraction failed with exit code {process.ExitCode}:\n{errors}");
                Main.Logger.Error($"7-Zip extraction failed with exit code {process.ExitCode}:\n{errors}");
            }
        }
    }
}