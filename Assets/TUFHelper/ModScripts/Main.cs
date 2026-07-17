using System.Collections.Generic;
using HarmonyLib;
using System.IO;
using System.Reflection;
using System.Threading;
using TUFHelper.ModScripts.Helpers;
using TUFHelper.Utils;
using UnityEngine;
using static UnityModManagerNet.UnityModManager;
using System;
using System.Linq;
using DG.Tweening;
using System.Net.Http;
using UnityModManagerNet;

namespace TUFHelper 
{
    public static class Main
    {
        internal static readonly HttpClient Client = new();

        internal static string modVersion = "2.6.2";

        internal static ModEntry.ModLogger Logger;
        internal static ModEntry ModEntry;
        internal static Harmony Harmony;
        internal static Setting Setting;

        internal static DownloadedLevelsFile DownloadedLevels;

        internal static AssetBundle assets, scenes;
        internal static bool isInTUFHelper = false;
        
        internal static SynchronizationContext mainThread;

        public static void Initialize(ModEntry modEntry)
        {
            ModEntry = modEntry;
            Logger = modEntry.Logger;

            foreach (var file in Directory.GetFiles(ModEntry.Path, "*.dll"))
            {
                if (file.EndsWith("TUFHelper.dll"))
                    continue;

                try
                {
                    Assembly.LoadFrom(file);
                    Main.Logger.Log("Loaded assembly from " + file);
                }
                catch (Exception ex)
                {
                    Main.Logger.Error($"Failed to load supplementary dependency DLL ({Path.GetFileName(file)}): {ex.Message}");
                }
            }

            string platformSuffix = Application.platform switch
            {
                RuntimePlatform.WindowsPlayer => "win",
                RuntimePlatform.OSXPlayer => "mac",
                RuntimePlatform.LinuxPlayer => "linux",
                _ => throw new ArgumentOutOfRangeException(nameof(Application.platform))
            };

            string legacyAssetsFolder = Path.Combine(modEntry.Path, "assets");
            string platformAssetsFolder = Path.Combine(modEntry.Path, platformSuffix);
            string bundleFolder = HasBundles(legacyAssetsFolder) ? legacyAssetsFolder : platformAssetsFolder;
            string assetsPath = Path.Combine(bundleFolder, "tuf_assets.bundle");
            string scenesPath = Path.Combine(bundleFolder, "tuf_scenes.bundle");

            if (!File.Exists(assetsPath) || !File.Exists(scenesPath))
            {
                Logger.Error($"Asset bundles missing at: {bundleFolder}");
                return;
            }

            assets = AssetBundle.LoadFromFile(assetsPath);
            scenes = AssetBundle.LoadFromFile(scenesPath);

            if (assets == null || scenes == null)
            {
                Logger.Error("Failed to load AssetBundles (check Unity version compatibility).");
                return;
            }


            Main.Logger.Log("TUFHelper assets and scenes loaded successfully.");
            DOTween.SetTweensCapacity(10000, 500);
            BundleFontFixer.Init();

            modEntry.Info.Version = modVersion;
            modEntry.Info.DisplayName = "TUFHelper";
            modEntry.Info.Id = "TUFHelper";
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            AdofaiTweaksAPI.Init();

            Application.wantsToQuit += () =>
            {
                return true;
            };
            mainThread = SynchronizationContext.Current;

            mainThread = SynchronizationContext.Current;

            try
            {
                Setting = Setting.LoadFromJson(modEntry);

                if (Setting == null)
                {
                    Main.Logger.Error("The settings generation failed. Reverting to application memory defaults.");
                    Setting = new Setting();
                }
            }
            catch (Exception ex)
            {
                Main.Logger.Error("Critical failure during deserialization mapping:");
                Main.Logger.LogException(ex);
                Setting = new Setting();
            }

            try
            {
                DownloadedLevels = DownloadedLevelsFile.Load(Path.Combine(ModEntry.Path, "Levels.json"));
            }
            catch (Exception ex)
            {
                Main.Logger.Error("Critical failure during deserialization mapping:");
                Main.Logger.LogException(ex);
                DownloadedLevels = new(Path.Combine(ModEntry.Path, "Levels.json"));
            }

            LanguageManager.Init();

            IngameUIManager.Instance.Initialize();

            //var mod = UnityModManager.modEntries.FirstOrDefault(e => e.Info.Id == "XPerfect");
            //if (mod != null)
            //{
            //    Main.Logger.Log("XPerfect is detected");
            //}
        }

        private static bool HasBundles(string folder)
        {
            return File.Exists(Path.Combine(folder, "tuf_assets.bundle"))
                && File.Exists(Path.Combine(folder, "tuf_scenes.bundle"));
        }

        internal static bool OnToggle(ModEntry modEntry, bool value)
        {
            if(value)
            {
                Harmony = new Harmony(modEntry.Info.Id);
                Harmony.PatchAll(Assembly.GetExecutingAssembly());
            } 
            else
            {
                Main.Harmony.UnpatchAll();
                ADOBase.loader.LoadScene("scnLevelSelect");
            }
            return true;
        }

        public static string CreateLabeledTextField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(100));
            value = GUILayout.TextField(value, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            return value;
        }
        internal static void OnGUI(ModEntry modEntry)
        {
            //GUILayout.BeginHorizontal();

            //GUILayout.Label("Level Save Path");
            //Setting.LevelSaveFolder = GUILayout.TextField(Setting.LevelSaveFolder, GUILayout.MinWidth(500));

            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            //Setting.StartWithGame = GUILayout.Toggle(Setting.StartWithGame, "Start With Game");
            //GUILayout.EndHorizontal();

            //GUILayout.BeginVertical("box");
            //GUILayout.Label("In-game Overlayer");

            //Setting.ShowTUFHelperOverlayer = GUILayout.Toggle(Setting.ShowTUFHelperOverlayer, "Show TUFHelper Overlayer");
            //Setting.ShowIngameSpeed = GUILayout.Toggle(Setting.ShowIngameSpeed, "Show In-game Speed Text");
            //Setting.ShowIngamePPCounter = GUILayout.Toggle(Setting.ShowIngamePPCounter, "Show In-game PP Counter");
            //Setting.ShowIngameLeaderboard = GUILayout.Toggle(Setting.ShowIngameLeaderboard, "Show In-game Leaderboard");
            //Setting.ShowIngameLevelInfo = GUILayout.Toggle(Setting.ShowIngameLevelInfo, "Show In-game Level Info");

            //if (!Setting.OverlayerElementsPositions.ContainsKey("IngameLeaderboard"))
            //    Setting.OverlayerElementsPositions["IngameLeaderboard"] = new();

            //GUILayout.Label($"Leaderboard Scale: {Setting.OverlayerElementsPositions["IngameLeaderboard"].Scale:F2}");
            //Setting.OverlayerElementsPositions["IngameLeaderboard"].Scale = Mathf.Round(
            //    GUILayout.HorizontalSlider(
            //        Setting.OverlayerElementsPositions["IngameLeaderboard"].Scale,
            //        0.5f,
            //        2.0f,
            //        GUILayout.Width(300)
            //    ) * 100f
            //) / 100f;

            //if (!Setting.OverlayerElementsPositions.ContainsKey("PPDisplayer"))
            //    Setting.OverlayerElementsPositions["PPDisplayer"] = new();

            //GUILayout.Label($"PP Displayer Scale: {Setting.OverlayerElementsPositions["PPDisplayer"].Scale:F2}");
            //Setting.OverlayerElementsPositions["PPDisplayer"].Scale = Mathf.Round(
            //    GUILayout.HorizontalSlider(
            //        Setting.OverlayerElementsPositions["PPDisplayer"].Scale,
            //        0.5f,
            //        2.0f,
            //        GUILayout.Width(300)
            //    ) * 100f
            //) / 100f;


            //GUILayout.EndVertical();
        }



        internal static void OnSaveGUI(ModEntry modEntry)
        {
            Setting?.Save(modEntry);
        }

        private static Dictionary<string, Sprite> _cachedSprites = new();
        private static Dictionary<string, string> _resolvedAssetNames = new();
        private static HashSet<string> _missingSprites = new();
        public static Sprite GetSpriteFromAssets(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || assets == null)
            {
                return null;
            }

            if (_cachedSprites.TryGetValue(path, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Sprite sprite = assets.LoadAsset<Sprite>(path);
            if (sprite == null)
            {
                string resolvedPath = ResolveAssetName(path);
                if (!string.IsNullOrEmpty(resolvedPath))
                {
                    sprite = assets.LoadAsset<Sprite>(resolvedPath);
                }
            }

            if (sprite != null)
            {
                _cachedSprites[path] = sprite;
            }
            else if (_missingSprites.Add(path))
            {
                Logger?.Error($"Sprite not found in TUFHelper AssetBundle: {path}");
            }

            return sprite;
        }

        private static string ResolveAssetName(string path)
        {
            if (_resolvedAssetNames.TryGetValue(path, out string cachedName))
            {
                return cachedName;
            }

            string normalizedPath = NormalizeAssetPath(path);
            string fileName = Path.GetFileName(normalizedPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(normalizedPath);

            string resolved = assets.GetAllAssetNames()
                .FirstOrDefault(assetName => NormalizeAssetPath(assetName) == normalizedPath)
                ?? assets.GetAllAssetNames()
                    .FirstOrDefault(assetName => Path.GetFileName(NormalizeAssetPath(assetName)) == fileName)
                ?? assets.GetAllAssetNames()
                    .FirstOrDefault(assetName => Path.GetFileNameWithoutExtension(NormalizeAssetPath(assetName)) == fileNameWithoutExtension)
                ?? assets.GetAllAssetNames()
                    .FirstOrDefault(assetName => Path.GetFileNameWithoutExtension(NormalizeAssetPath(assetName)).Contains(fileNameWithoutExtension))
                ?? assets.GetAllAssetNames()
                    .FirstOrDefault(assetName => NormalizeAssetPath(assetName).EndsWith(normalizedPath, StringComparison.Ordinal));

            if (!string.IsNullOrEmpty(resolved))
            {
                _resolvedAssetNames[path] = resolved;
            }

            return resolved;
        }

        private static string NormalizeAssetPath(string path)
        {
            return (path ?? string.Empty)
                .Replace('\\', '/')
                .Trim()
                .TrimStart('/')
                .ToLowerInvariant();
        }
    }
}
