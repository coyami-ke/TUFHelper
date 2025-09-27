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
            { 1, "T1" },
            { 2, "T2H" },
            { 3, "T2" },
            { 4, "T3H" },
            { 5, "T3" },
            { 6, "T4" },
            { 7, "Epic" },
            { 8, "Ornamental" },
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
