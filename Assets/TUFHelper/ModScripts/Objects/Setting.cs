using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityModManagerNet;

namespace TUFHelper
{
    public class Setting : UnityModManager.ModSettings
    {

        public int orderMode = 0; // 0 -> id, 1 -> difficulty
        public int orderByIDMode = -1; // 1 -> down, -1 -> up
        public int orderByDifficultyMode = 1; // 1 -> down, -1 -> up

        public bool showLegacyRating = false, showUnratedLevels = false;

        public List<int> favouritesID = new List<int>();

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            if (modEntry == null)
            {
                return;
            }
            var filepath = GetPath(modEntry);
            try
            {
                using (var writer = new StreamWriter(filepath))
                {
                    var serializer = new XmlSerializer(GetType());
                    serializer.Serialize(writer, this);
                }
            }
            catch (Exception e)
            {
                modEntry.Logger.Error($"Can't save {filepath}.");
                modEntry.Logger.LogException(e);
            }
        }

        public override string GetPath(UnityModManager.ModEntry modEntry)
        {
            return Path.Combine(modEntry.Path, GetType().Name + ".xml");
        }

    }
}
