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

namespace TUFHelper 
{
    public static class Main
    {
        internal static string modVersion = "2.2.0";

        internal static ModEntry.ModLogger Logger;
        internal static ModEntry ModEntry;
        internal static Harmony Harmony;
        internal static Setting Setting;

        internal static AssetBundle assets, scenes;
        internal static bool isInTUFHelper = false;
        
        internal static List<string> removeLevels = new();
        internal static SynchronizationContext mainThread;

        private static string FindTUFHelperPath()
        {
            var modDirectories = Directory.GetDirectories(Path.Combine(Environment.CurrentDirectory, "Mods"), "*", SearchOption.AllDirectories);
            foreach (var dir in modDirectories)
            {
                if (Path.GetFileName(dir).StartsWith("TUFHelper", StringComparison.OrdinalIgnoreCase))
                {
                    return dir;
                }
            }
            return null;
        }
        public static void Initialize(ModEntry modEntry)
        {
            
            ModEntry = modEntry;
            Logger = modEntry.Logger;

            string tufHelperPath = FindTUFHelperPath();

            if (string.IsNullOrEmpty(tufHelperPath))
            {
                Main.Logger.Log("TUFHelper directory not found.");
                return;
            }

            string assetsPath = Path.Combine(tufHelperPath, "assets", "tuf_assets.bundle");
            string scenesPath = Path.Combine(tufHelperPath, "assets", "tuf_scenes.bundle");

            if (!File.Exists(assetsPath) || !File.Exists(scenesPath))
            {
                Main.Logger.Log("Asset bundles not found in TUFHelper directory.");
                return;
            }

            assets = AssetBundle.LoadFromFile(assetsPath);
            scenes = AssetBundle.LoadFromFile(scenesPath);


            Main.Logger.Log("TUFHelper assets and scenes loaded successfully.");

            modEntry.Info.Version = modVersion;
            modEntry.Info.DisplayName = "TUFHelper";
            modEntry.Info.Id = "TUFHelper";
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            AdofaiTweaksAPI.Init();

            Application.wantsToQuit += () =>
            {
                foreach (var levelPath in removeLevels)
                    Directory.Delete(levelPath, true);
                return true;
            };
            mainThread = SynchronizationContext.Current;

            Setting = new Setting();
            var settings = Setting.LoadFromJson(modEntry);
            if (settings != null) Setting = settings;
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
                Harmony.UnpatchAll();
                ADOBase.LoadScene("scnLevelSelect");
            }
            return true;
        }

        internal static void OnGUI(ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label("Level Save Path");
            Setting.LevelSaveFolder = GUILayout.TextField(Setting.LevelSaveFolder, GUILayout.MinWidth(500));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Setting.StartWithGame = GUILayout.Toggle(Setting.StartWithGame, "Start With Game");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Setting.ShowTUFHelperOverlayer = GUILayout.Toggle(Setting.ShowTUFHelperOverlayer, "Show TUFHelper Overlayer");
            GUILayout.EndHorizontal();
        }


        internal static void OnSaveGUI(ModEntry modEntry)
        {
            Setting?.Save(modEntry);
        }

        private static Dictionary<string, Sprite> _cachedSprites = new();
        public static Sprite GetSpriteFromAssets(string path)
        {
            if (_cachedSprites.ContainsKey(path))
            {
                return _cachedSprites[path];
            }
            else
            {
                Sprite sprite = assets.LoadAsset<Sprite>(path);
                _cachedSprites.Add(path, sprite);
                return sprite;
            }
        }
    }
}
