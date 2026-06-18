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

    public class Setting : UnityModManager.ModSettings
    {
        public string LevelSaveFolder { get; set; } = null;
        public int MinDiff { get; set; } = 1;
        public int MaxDiff { get; set; } = 60;
        public int MinQDiff { get; set; } = 1;
        public int MaxQDiff { get; set; } = 10;
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
        public Dictionary<string, TransformOverlayerElement> OverlayerElementsPositions { get; set; } = new();
        public bool ShowTUFHelperOverlayer { get; set; } = true;
        public bool ShowIngameLeaderboard { get; set; } = true;
        public bool ShowIngameSpeed { get; set; } = true;
        public bool ShowIngamePPCounter { get; set; } = true;
        public bool ShowIngameLevelInfo { get; set; } = true;
        public bool IsShowedKeyviewerError { get; set; } = false;
        public bool IsShowedFmodError { get; set; } = false;

        public Setting()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                LevelSaveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TUFHelper", "Levels");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Fallback check if Main.ModEntry isn't loaded during early constructor assembly instantiation
                string modPath = Main.ModEntry != null ? Main.ModEntry.Path : AppDomain.CurrentDomain.BaseDirectory;
                LevelSaveFolder = Path.Combine(modPath, "SavedLevels");
            }
        }

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            if (modEntry == null)
            {
                Main.Logger.Error("The mod entry is null");
                return;
            }
            var filepath = GetPath(modEntry);
            try
            {
                // Force formatting indented so it's readable and writes cleanly
                File.WriteAllText(filepath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch (Exception e)
            {
                modEntry.Logger.Error($"Can't save {filepath}.");
                modEntry.Logger.LogException(e);
            }
        }

        // Changed to a static helper method so it can load independently 
        // without requiring a blank "new Setting()" instance first
        public static Setting LoadFromJson(UnityModManager.ModEntry modEntry)
        {
            if (modEntry == null) return new Setting();

            // Create a reliable path compilation string separate from internal UMM wrappers
            string filepath = Path.Combine(modEntry.Path, "Settings.json");
            Main.Logger.Log($"Attempting to load settings from: {filepath}");

            if (!File.Exists(filepath))
            {
                Main.Logger.Log("Settings file doesn't exist. Creating fresh default settings.");
                Setting newSettings = new Setting();
                newSettings.Save(modEntry); // Write a clean default template immediately
                return newSettings;
            }

            try
            {
                string text = File.ReadAllText(filepath);
                Setting loadedSettings = JsonConvert.DeserializeObject<Setting>(text);

                // Safety fallback if file is empty or corrupted JSON returns null
                return loadedSettings ?? new Setting();
            }
            catch (Exception ex)
            {
                modEntry.Logger.Error($"Can't load {filepath}. Reverting to default settings.");
                modEntry.Logger.LogException(ex);
                return new Setting();
            }
        }

        public override string GetPath(UnityModManager.ModEntry modEntry)
        {
            return Path.Combine(modEntry.Path, "Settings.json");
        }
    }
}