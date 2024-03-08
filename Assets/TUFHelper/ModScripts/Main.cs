using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityModManagerNet.UnityModManager;

namespace TUFHelper 
{
    public static class Main
    {

        internal static string modVersion = "0.0.1";

        internal static ModEntry.ModLogger Logger;
        internal static ModEntry ModEntry;
        internal static Harmony Harmony;
        internal static Setting Setting;

        internal static AssetBundle assets, scenes;
        internal static bool isInTUFHelper = false;

        public static void Initialize(ModEntry modEntry)
        {
            assets = AssetBundle.LoadFromFile(Path.Combine("Mods", "TUFHelper", "assets", "assets.bundle"));
            scenes = AssetBundle.LoadFromFile(Path.Combine("Mods", "TUFHelper", "assets", "scenes.bundle"));

            ModEntry = modEntry;
            Logger = modEntry.Logger;

            modEntry.Info.Version = modVersion;
            modEntry.Info.DisplayName = "TUFHelper";
            modEntry.Info.Id = "TUFHelper";
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

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
            if (GUILayout.Button("test"))
            {
                scnEditor.levelToOpenOnLoad = "";
                SceneManager.LoadScene("scnEditor");
            }
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
