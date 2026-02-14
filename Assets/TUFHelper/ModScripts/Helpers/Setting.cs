using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
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

        //public bool PlayBackgroundMusic { get; set; } = true;
        public int MinDiff { get; set; } = 1;
        public int MaxDiff { get; set; } = 60;

        public int MinQDiff { get; set; } = 1;
        public int MaxQDiff { get; set; } = 10;

        public float TUFHelperMusicVolume = 0.5f;
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
                LevelSaveFolder = Path.Combine(Main.FindTUFHelperPath(), "SavedLevels");
            }
        }
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            if (modEntry == null)
            {
                return;
            }
            var filepath = GetPath(modEntry);
            try
            {
                File.WriteAllText(filepath, JsonConvert.SerializeObject(this));
            }
            catch (Exception e)
            {
                modEntry.Logger.Error($"Can't save {filepath}.");
                modEntry.Logger.LogException(e);
            }
        }
        public Setting LoadFromJson(UnityModManager.ModEntry modEntry)
        {
            if (!File.Exists(GetPath(modEntry))) return new();
            if (modEntry == null) return null;
            try
            {
                return JsonConvert.DeserializeObject<Setting>(File.ReadAllText(GetPath(modEntry)));
            }
            catch (Exception ex)
            {
                modEntry.Logger.Error($"Can't save {GetPath(modEntry)}.");
                modEntry.Logger.LogException(ex);
                return null;
            }
        }
        public override string GetPath(UnityModManager.ModEntry modEntry)
        {
            return Path.Combine(modEntry.Path, "Settings.json");
        }
    }
}
