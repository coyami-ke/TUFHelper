using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using UnityModManagerNet;

namespace TUFHelper.Utils
{
    public class DownloadedLevel
    {
        public LevelListInfoElementJson LevelInfo { get; set; }
        public string NameFolder { get; set; }
    }
    public class Setting : UnityModManager.ModSettings
    {
        public string LevelSaveFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\TUFHelper\Levels";
        public bool PlayBackgroundMusic { get; set; } = true;
        public int MinDiff { get; set; } = 1;
        public int MaxDiff { get; set; } = 60;
        public AscendingOrDescending SortOrder { get; set; } = AscendingOrDescending.Descending;
        public int SortBy { get; set; } = 0;
        public List<string> SelectedSpecialDiffs { get; set; } = new();
        public bool ShowOnlyDownloaded { get; set; } = false;
        public bool StartWithGame { get; set; } = false;
        public List<DownloadedLevel> DownloadedLevels { get; set; } = new();

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
