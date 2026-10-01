using System;
using System.Collections.Generic;
using TUFHelper.ModScripts.Json;
using UnityEngine;

namespace TUFHelper.Utils
{
    public static class CurationHelper
    {
        public const string PATH_TO_CURATION_SPRITES = "Assets/TUFHelper/Assets/Sprites/Curation/";

        public static readonly Dictionary<int, string> IDRegister = new()
        {
            { 19, "C3" },
            { 18, "C2" },
            { 17, "C1" },
            { 16, "C0" },
            { 25, "O3" },
            { 24, "O2" },
            { 23, "O1" },
            { 26, "V3" },
            { 22, "V2" },
            { 21, "V1" },
            { 20, "V0" },
            { 4, "H2" },
            { 2, "H1" },
            { 7, "Epic" },
        };

        public static Sprite GetSprite(LevelListInfoElementCurationTypeJson curation)
        {
            if (curation == null || string.IsNullOrEmpty(curation.Name)) return null;
            return Main.GetSpriteFromAssets($"{PATH_TO_CURATION_SPRITES}{curation.Name}");
        }
        public static Sprite GetSpriteFromId(int typeId)
        {
            if (IDRegister.TryGetValue(typeId, out string spriteName))
            {
                return Main.GetSpriteFromAssets($"{PATH_TO_CURATION_SPRITES}{spriteName}");
            }

            return null;
        }
    }
}