using ADOFAIModdingHelper.ScriptableObjects;
using ADOFAIModdingHelper.Utilities;
using ADOFAIModdingHelper.Windows;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace ADOFAIModdingHelper.Toolbar
{
    public static class RunADOFAIToolbar
    {
        private static readonly Texture2D gearIcon = EditorGUIUtility.IconContent("SettingsIcon").image as Texture2D;
        const string k_ModToolsToolbarElementName = "ADOFAI Modding Helper/";

        [MainToolbarElement(k_ModToolsToolbarElementName + "Mods Tools", defaultDockPosition = MainToolbarDockPosition.Left)]
        static IEnumerable<MainToolbarElement> CreateModToolsBar()
        {
            var config = ModToolsConfig.Config;
            var setting = Setting.Config;
            string modId = ModInfo.Info.Id;

            yield return new MainToolbarLabel(new MainToolbarContent(modId, "Current Active Mod ID"));

            yield return new MainToolbarButton(new MainToolbarContent("Run", "Compile Everything and Run ADOFAI"), () =>
            {
                string dest = config.copyToDirectory
                    ? Path.Combine(Path.GetDirectoryName(setting.ADOFAIPath)!, "Mods", modId)
                    : null;
                config.BuildMod(dest);
            });

            yield return new MainToolbarButton(new MainToolbarContent("FRun", "Quick Run ADOFAI without compiling"), () => config.RunApp(true));

            yield return new MainToolbarButton(new MainToolbarContent(gearIcon, "Open Mod Config"), () => ModConfigWindow.OpenBuild());
        }

        [MainToolbarElement(k_ModToolsToolbarElementName + "Decompiler", defaultDockPosition = MainToolbarDockPosition.Right)]
        static IEnumerable<MainToolbarElement> CreateDecompilerToolsBar()
        {
            var setting = Setting.Config;

            yield return new MainToolbarButton(new MainToolbarContent("Open Decompiler", "Open The Decompiler"), () =>
            {
                if (setting == null || string.IsNullOrEmpty(setting.DecompilerPath) || !File.Exists(setting.DecompilerPath))
                {
                    UnityEngine.Debug.LogWarning("Cannot open decompiler: The configured path is invalid or the file does not exist.");
                    return;
                }

                string gameDirectory = Path.GetDirectoryName(setting.DecompilerPath);

                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = setting.DecompilerPath,
                    WorkingDirectory = gameDirectory,
                    UseShellExecute = true
                };

                System.Diagnostics.Process.Start(startInfo);
            });
        }
    }
}