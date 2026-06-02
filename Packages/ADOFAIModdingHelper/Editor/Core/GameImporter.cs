//Original Code from https://github.com/ADOFAI-gg/ADOFAI-Modding-Toolkit
using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.ScriptableObjects;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ADOFAIModdingHelper.Core
{
    public class GameImporter
    {
        private readonly string _packagePath = Path.Combine(Directory.GetCurrentDirectory(), "Packages", "adofai");

        public string GamePath;
        public string GameExecutable;
        public string ModdingRoot;
        public string DataPath;

        private const string GameImportScheduledKey = "ADOFAI_Modding_Helper:ImportScheduled";

        private static readonly string[] BlacklistedAssemblyNames = {
            "BepInEx.Harmony.dll", "RuntimeUnityEditor.Core.dll", "RuntimeUnityEditor.Bepin5.dll"
        };

        static GameImporter()
        {
            EditorApplication.delayCall += () =>
            {
                var Settings = Setting.Config;
                if (EditorPrefs.GetBool(GameImportScheduledKey))
                {
                    Settings.Importer.SetGamePath(Settings.ADOFAIPath);
                    Settings.Importer.Import();
                }
            };
        }

        public static bool ValidateGamePath(string gamePath)
        {
            var filename = Path.GetFileName(gamePath);
#if UNITY_EDITOR_OSX
            if (filename == "ADanceOfFireAndIce.app" && Directory.Exists(gamePath))
            {
                return true;
            }
#elif UNITY_EDITOR_LINUX
			if (filename == "ADanceOfFireAndIce" && File.Exists(gamePath)) {
				return true;
			}
#else
            if (filename == "A Dance of Fire and Ice.exe" && File.Exists(gamePath))
            {
                return true;
            }
#endif
            EditorUtility.DisplayDialog("Invalid file", "Please select valid adofai executable file", "OK");

            return false;
        }

        [UsedImplicitly]
        public void SetGamePath(string gamePath)
        {
            if (!ValidateGamePath(gamePath))
            {
                throw new ArgumentException("Invalid game path");
            }

#if UNITY_EDITOR_OSX
            GamePath = gamePath;
            GameExecutable = gamePath;
            ModdingRoot = Path.GetDirectoryName(gamePath);
            DataPath = Path.Combine(gamePath, "Contents", "Resources", "Data");
#elif UNITY_EDITOR_LINUX
			GamePath = Path.GetDirectoryName(gamePath)!;
			GameExecutable = gamePath;
			ModdingRoot = GamePath;
			DataPath = Path.Combine(GamePath, "ADanceOfFireAndIce_Data");
#else
            GamePath = Path.GetDirectoryName(gamePath)!;
            GameExecutable = gamePath;
            ModdingRoot = GamePath;
            DataPath = Path.Combine(GamePath, "A Dance of Fire and Ice_Data");
#endif
        }

        public void Import()
        {
            var scheduled = EditorPrefs.GetBool(GameImportScheduledKey);
            EditorPrefs.SetBool(GameImportScheduledKey, false);
            if (Directory.Exists(_packagePath) && !scheduled)
            {
                if (EditorUtility.DisplayDialog("Package already exists",
                        "The package is already created. You should restart the editor after deleting the package to properly load it. Do you want to restart unity editor?",
                        "OK",
                        "Cancel"))
                {
                    Directory.Delete(_packagePath, true);
                    EditorPrefs.SetBool(GameImportScheduledKey, true);
                    AssetDatabase.SaveAssets();

                    // restart editor
                    EditorApplication.OpenProject(Directory.GetCurrentDirectory());
                    return;
                }
            }

            CreatePackage();
            CopyAssemblies();
            AssetDatabase.Refresh();

            Debug.Log("");
        }

        private void CreatePackage()
        {
            Directory.CreateDirectory(_packagePath);
            File.WriteAllText(Path.Combine(_packagePath, "package.json"),
                EditorJsonUtility.ToJson(
                    new PackageManifest { name = "adofai", version = "0.0.1", displayName = "A Dance of Fire and Ice" },
                    true));
        }

        private void CopyAssemblies()
        {
            var assemblyPaths = new List<string>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    assemblyPaths.Add(asm.Location);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            var assemblyNames = (from path in assemblyPaths select Path.GetFileName(path)).ToList();
            var managedDir = Path.Combine(DataPath, "Managed");
            var bepInExDir = Path.Combine(ModdingRoot, "BepInEx");

            var dirs = new[] { managedDir, bepInExDir };

            foreach (var dir in dirs)
            {
                if (!Directory.Exists(dir)) continue;

                var files = Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var name = Path.GetFileName(file);
                    if (assemblyNames.Contains(name)) continue;
                    if (BlacklistedAssemblyNames.Contains(name)) continue;

                    var dest = Path.Combine(_packagePath, name);
                    File.Copy(file, dest, true);
                    File.WriteAllText(dest + ".meta", $@"
fileFormatVersion:
    guid: {GUID.Generate()}
PluginImporter:
    isExplicitlyReferenced: 1
    validateReferences: 1");
                }
            }
        }
    }
}