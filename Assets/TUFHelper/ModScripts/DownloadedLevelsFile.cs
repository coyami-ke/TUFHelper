using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TUFHelper.ModScripts.Json;

namespace TUFHelper
{
    public class DownloadedLevelsFile
    {
        public List<LevelListInfoElementJson> Levels { get; set; } = new();

        [JsonIgnore]
        public string PathToSaveFile { get; set; }

        // Default constructor required for Newtonsoft.Json deserialization
        public DownloadedLevelsFile() { }

        public DownloadedLevelsFile(string path)
        {
            PathToSaveFile = path;
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(PathToSaveFile))
            {
                Main.Logger?.Log("[DownloadedLevelsFile] Save skipped: PathToSaveFile is null or empty.");
                return;
            }

            File.WriteAllText(PathToSaveFile, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public void SaveLevel(LevelListInfoElementJson levelInfo)
        {
            if (levelInfo == null) return;

            var levelWithSameID = Levels.FirstOrDefault(e => e.ID == levelInfo.ID);

            if (levelWithSameID != null)
            {
                Levels.Remove(levelWithSameID);
            }

            Levels.Add(levelInfo);

            Save();
        }

        public static DownloadedLevelsFile Load(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var file = JsonConvert.DeserializeObject<DownloadedLevelsFile>(json);

                    if (file == null)
                    {
                        file = new DownloadedLevelsFile(path);
                    }
                    else
                    {
                        file.PathToSaveFile = path;
                    }

                    return file;
                }
                catch (System.Exception ex)
                {
                    Main.Logger?.Log($"[DownloadedLevelsFile] Error loading file at {path}: {ex.Message}");
                    return new DownloadedLevelsFile(path);
                }
            }
            else
            {
                return new DownloadedLevelsFile(path);
            }
        }
    }
}