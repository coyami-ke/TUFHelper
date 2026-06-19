using System;
using System.Collections.Generic;
using System.Text;

namespace TUFHelper.Utils
{
    public static class CurationHelper
    {
        public const string PATH_TO_CURATION_SPRITES = "Assets/TUFHelper/Assets/Sprites/Curation/";
        public static readonly Dictionary<int, string> CurationIconsDictionary = new()
        {
            // --- Chart Group ---
            { 16, "C0" },
            { 17, "C1" },
            { 18, "C2" },
            { 19, "C3" },

            // --- Ornamental Group ---
            { 23, "O1" },
            { 24, "O2" },
            { 25, "O3" },

            // --- VFX Group ---
            { 20, "V0" },
            { 21, "V1" },
            { 22, "V2" },
            { 26, "V3" },

            // --- Misc / Legacy Group ---
            { 2,  "H1" },
            { 4,  "H2" },
            { 7,  "Epic" }
        };

        public static string GetSpriteFromId(int id)
        {
            if (CurationIconsDictionary.ContainsKey(id))
            {
                return $"{PATH_TO_CURATION_SPRITES}{CurationIconsDictionary[id]}.png";
            }
            else return $"{PATH_TO_CURATION_SPRITES}Unknown.png";
        }
    }
}
