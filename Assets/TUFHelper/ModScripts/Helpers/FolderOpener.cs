using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TUFHelper.Utils
{
    public static class FolderOpener
    {
        public static void OpenFolder(string folderPath)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start("explorer.exe", folderPath.Replace("/", "\\"));
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", folderPath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", folderPath);
                }
                else
                {
                    throw new PlatformNotSupportedException("Unsupported OS");
                }
            }
            catch (Exception ex)
            {
                Main.Logger.Error("Failed to open folder: " + ex.Message);
            }
        }
    }
}