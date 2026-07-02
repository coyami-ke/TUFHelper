using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using UnityEngine;
using UnityModManagerNet;

namespace TUFHelper.Utils
{
    public class TransformOverlayerElement
    {
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float Scale { get; set; } = 1;
    }

    public class DownloadedLevel
    {
        public LevelListInfoElementJson LevelInfo { get; set; }
        public string NameFolder { get; set; }
        public CustomLevelInfoJson LocalData { get; set; }
    }

    public class Setting
    {
        public string LevelSaveFolder { get; set; } = null;
        public int MinDiff { get; set; } = 1;
        public int MaxDiff { get; set; } = 60;
        public int MinQDiff { get; set; } = 1;
        public int MaxQDiff { get; set; } = 11;
        public float TUFHelperMusicVolume { get; set; } = 0.5f; // Converted to property for consistent serialization
        public AscendingOrDescending SortOrder { get; set; } = AscendingOrDescending.Descending;
        public int SortBy { get; set; } = 0;
        public List<string> SelectedSpecialDiffs { get; set; } = new();
        public List<string> SelectedQDiifs { get; set; } = new();
        public bool ShowOnlyDownloaded { get; set; } = false;
        public bool ShowOnlyFavorites { get; set; } = false;
        public bool StartWithGame { get; set; } = true;
        public bool GroupByFolders { get; set; } = false;
        public List<DownloadedLevel> DownloadedLevels { get; set; } = new();
        public HashSet<int> FavoriteLevels { get; set; } = new();
        public List<LevelFolder> LevelFolders { get; set; } = new();
        //public Dictionary<string, TransformOverlayerElement> OverlayerElementsPositions { get; set; } = new();
        public Dictionary<string, IngameElementModel> IngameElementsSettings { get; set; } = new();
        public bool ShowTUFHelperOverlayer { get; set; } = true;
        //public bool ShowIngameLeaderboard { get; set; } = true;
        //public bool ShowIngameSpeed { get; set; } = true;
        //public bool ShowIngamePPCounter { get; set; } = true;
        //public bool ShowIngameLevelInfo { get; set; } = true;
        public bool IsShowedKeyviewerError { get; set; } = false;
        public bool IsShowedFmodError { get; set; } = false;
        public string Language { get; set; } = "Auto";

        public Setting()
        {
            // Establish defaults if paths are evaluated natively
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                LevelSaveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TUFHelper", "Levels");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string modPath = Main.ModEntry != null ? Main.ModEntry.Path : AppDomain.CurrentDomain.BaseDirectory;
                LevelSaveFolder = Path.Combine(modPath, "SavedLevels");
            }
        }

        public void Save(UnityModManager.ModEntry modEntry)
        {
            if (modEntry == null)
            {
                Main.Logger?.Error("The mod entry is null");
                return;
            }
            var filepath = GetPath(modEntry);
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filepath, json);
            }
            catch (Exception e)
            {
                modEntry.Logger.Error($"Can't save {filepath}.");
                modEntry.Logger.LogException(e);
            }
        }
        public static Setting LoadFromJson(UnityModManager.ModEntry modEntry)
        {
            if (modEntry == null) return new Setting();

            string filepath = Path.Combine(modEntry.Path, "Settings.json");

            if (!File.Exists(filepath))
            {
                Main.Logger?.Log($"Settings file doesn't exist at path. Creating a fresh instance: {filepath}");
                Setting newSettings = new Setting();
                newSettings.Save(modEntry);
                return newSettings;
            }

            try
            {
                string text = File.ReadAllText(filepath);
                if (string.IsNullOrWhiteSpace(text))
                {
                    Main.Logger?.Log("Settings file was empty. Reverting to default values.");
                    return new Setting();
                }

                Setting loadedSettings = JsonConvert.DeserializeObject<Setting>(text);
                return loadedSettings ?? new Setting();
            }
            catch (Exception ex)
            {
                modEntry.Logger.Error($"Can't read setup parameters from {filepath}. Reverting to defaults safely.");
                modEntry.Logger.LogException(ex);
                return new Setting();
            }
        }

        public string GetPath(UnityModManager.ModEntry modEntry)
        {
            return Path.Combine(modEntry.Path, "Settings.json");
        }
    }
}
