using System.Collections.Generic;
using HarmonyLib;
using System.IO;
using System.Reflection;
using System.Threading;
using TUFHelper.ModScripts.Helpers;
using TUFHelper.Utils;
using UnityEngine;
using static UnityModManagerNet.UnityModManager;

namespace TUFHelper 
{
    public static class Main
    {

        internal static Dictionary<string, PlayerInfo> playerData = new Dictionary<string, PlayerInfo>();
        internal static string modVersion = "1.1.0";

        internal static ModEntry.ModLogger Logger;
        internal static ModEntry ModEntry;
        internal static Harmony Harmony;
        internal static Setting Setting;

        internal static AssetBundle assets, scenes;
        internal static bool isInTUFHelper = false;
        
        internal static List<string> removeLevels = new List<string>();
        internal static SynchronizationContext mainThread;

        public static void Initialize(ModEntry modEntry)
        {
            assets = AssetBundle.LoadFromFile(Path.Combine("Mods", "TUFHelper", "assets", "tuf_assets.bundle"));
            scenes = AssetBundle.LoadFromFile(Path.Combine("Mods", "TUFHelper", "assets", "tuf_scenes.bundle"));

            ModEntry = modEntry;
            Logger = modEntry.Logger;

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
            Setting = ModSettings.Load<Setting>(modEntry);
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
            Setting.levelSaveFolder = GUILayout.TextField(Setting.levelSaveFolder, GUILayout.MinWidth(500));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        internal static void OnSaveGUI(ModEntry modEntry)
        {
            if (Setting != null)
            {
                Setting.Save(modEntry);
            }
        }

    }
}
