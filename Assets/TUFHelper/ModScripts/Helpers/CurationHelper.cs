using System;
using System.Collections.Generic;
using TUFHelper.ModScripts.Json;

namespace TUFHelper.Utils
{
    public static class CurationHelper
    {
        public const string PATH_TO_CURATION_SPRITES = "Assets/TUFHelper/Assets/Sprites/Curation/";

        public static readonly Dictionary<int, string> CurationIconsDictionary = new()
        {
            { 1, "T1" },
            { 2, "T2" },
            { 3, "T2H" },
            { 4, "T3" },
            { 5, "T3H" },
            { 6, "T4" },
            { 7, "Epic" },
            { 8, "Ornamental" },

            { 16, "T1" },
            { 17, "T2" },
            { 18, "T3" },
            { 19, "T4" },
            { 20, "T2" },
            { 21, "T3" },
            { 22, "T4" },
            { 23, "Ornamental" },
            { 24, "Ornamental" },
            { 25, "Ornamental" },
            { 26, "Epic" }
        };

        public static string GetSpriteFromCuration(LevelListInfoElementCurationJson curation)
        {
            if (curation == null)
            {
                return null;
            }

            string name = NormalizeIconName(curation.Type?.Icon) ?? NormalizeIconName(curation.Type?.Name);
            if (!string.IsNullOrEmpty(name))
            {
                return $"{PATH_TO_CURATION_SPRITES}{name}.png";
            }

            int? id = curation.TypeID ?? curation.Type?.ID;
            return id.HasValue ? GetSpriteFromId(id.Value) : null;
        }

        public static string GetSpriteFromId(int id)
        {
            return CurationIconsDictionary.TryGetValue(id, out string icon)
                ? $"{PATH_TO_CURATION_SPRITES}{icon}.png"
                : null;
        }

        private static string NormalizeIconName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            value = value.Trim();
            if (value.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(0, value.Length - 4);
            }

            string lower = value.ToLowerInvariant();
            if (lower.Contains("ornamental"))
            {
                return "Ornamental";
            }

            if (lower.Contains("epic"))
            {
                return "Epic";
            }

            string upper = value.ToUpperInvariant()
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("_", string.Empty);

            return upper switch
            {
                "T1" => "T1",
                "T2" => "T2",
                "T2H" => "T2H",
                "T3" => "T3",
                "T3H" => "T3H",
                "T4" => "T4",
                _ => null
            };
        }
    }
}
