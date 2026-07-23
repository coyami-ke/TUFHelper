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
using TUFHelper.ModScripts.Json;
using Newtonsoft.Json;

namespace TUFHelper
{
    public class DownloadedLevelsFile
    {
        public List<LevelListInfoElementJson> Levels { get; set; } = new();
        [JsonIgnore]
        private string PathToSaveFile { get; }

        public DownloadedLevelsFile(string path)
        {
            PathToSaveFile = path;
        }

        public void Save()
        {
            File.WriteAllText(PathToSaveFile, JsonConvert.SerializeObject(this));
        }
        public void SaveLevel(LevelListInfoElementJson levelInfo)
        {
            var levelWithSameID = Levels.FirstOrDefault(e => e.ID == levelInfo.ID);

            if (levelWithSameID != null)
            {
                Levels.Remove(levelWithSameID);
            }

            Levels.Add(levelInfo);
        }

        public static DownloadedLevelsFile Load(string path)
        {
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<DownloadedLevelsFile>(File.ReadAllText(path));
            }
            else
            {
                return new(path);
            }
        }
    }
}
