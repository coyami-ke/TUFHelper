using System.Diagnostics;
using System.IO;

namespace TUFHelper.Utils
{
    public static class ZipHelper
    {
        public static void Unzip(string zipFilePath, string extractFolderPath)
        {
            // Ensure the output directory ends with a backslash
            if (!extractFolderPath.EndsWith("\\"))
            {
                extractFolderPath += "\\";
            }

            string tufhelperPath = Main.FindTUFHelperPath();

            // Path to the 7-Zip executable
            // If 7z.exe is not in your system's PATH, provide the full path to the executable
            string sevenZipPath = Path.Combine(tufhelperPath, "7zip", "7z.exe");

            // Setup the process with the ProcessStartInfo class
            ProcessStartInfo proccessStartInfo = new()
            {
                UseShellExecute = false,
                FileName = sevenZipPath,
                CreateNoWindow = true, // Set this to false if you want to see the 7-Zip window
                RedirectStandardOutput = true,
                RedirectStandardError = true,

                // Set arguments for the extraction command
                // Using 'x' for full path extraction, replace with 'e' if you don't want to preserve directory structure
                Arguments = $"x \"{zipFilePath}\" -o\"{extractFolderPath}\" -y"
            };

            using Process process = Process.Start(proccessStartInfo);
            // Read the output (or errors)
            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();

            process.WaitForExit(); // Wait for the extraction to finish
        }
    }
}