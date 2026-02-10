using System;
using System.Collections.Generic;

namespace TUFHelper.Utils
{
    public static class TagSpriteHelper
    {
        public static readonly Dictionary<string, string> SpriteRegister = new()
        {
            // ===== Play Style =====
            { "Pseudo", "Icon_Play_Pseudo" },
            { "Rolling", "Icon_Play_Rolling" },
            { "Indexing", "Icon_Play_Index" },
            { "Tech", "Icon_Play_Tech" },
            { "Key Count", "Icon_Play_KeyCount" },
            { "Key Count+", "Icon_Play_KeyCountPlus" },
            { "Feetdex", "Icon_Play_Feetdex" },
            { "Feet Switch", "Icon_Play_FeetSwitch" },

            // ===== Key Limit =====
            { "1 Key Limit", "Icon_Limit_1" },
            { "2 Key Limit", "Icon_Limit_2" },
            { "4 Key Limit", "Icon_Limit_4" },
            { "8 Key Limit", "Icon_Limit_8" },
            { "10 Key Limit", "Icon_Limit_10" },
            { "12 Key Limit", "Icon_Limit_12" },
            { "16 Key Limit", "Icon_Limit_16" },
            { "Overlay Allowed", "Icon_Limit_Overlay" },
            { "2-Hand Pseudos", "Icon_Limit_2Hand" },
            { "Onhand/Offhand Limit", "Icon_Limit_OnOff" },
            { "Variable Key Limit", "Icon_Limit_Variable" },

            // ===== Judgement =====
            { "Judgement Limit", "Icon_Judge_Limit" },
            { "HP Bar", "Icon_Judge_HP" },
            { "Detailed Judgement", "Icon_Judge_Detail" },

            // ===== Gimmick =====
            { "Free Roam", "Icon_Gimmick_FreeRoam" },
            { "Multi Track", "Icon_Gimmick_MultiTrack" },
            { "Math", "Icon_Gimmick_Math" },
            { "RPG", "Icon_Gimmick_RPG" },
            { "Memorization", "Icon_Gimmick_Memo" },
            { "Unorthodox Reading", "Icon_Gimmick_Unorthodox" },
            { "Arrow Key", "Icon_Gimmick_Arrow" },

            // ===== VFX =====
            { "Full VFX", "Icon_VFX_Full" },
            { "Camera", "Icon_VFX_Camera" },
            { "Filters", "Icon_VFX_Filter" },
            { "Non-VFX", "Icon_VFX_None" },
            { "Decorations", "Icon_VFX_Deco" },
            { "Low VFX", "Icon_VFX_Low" },

            // ===== Length =====
            { "Tiny", "Icon_Time_Tiny" },
            { "30+ Seconds", "Icon_Time_30s" },
            { "1+ Minute", "Icon_Time_1m" },
            { "2+ Minutes", "Icon_Time_2m" },
            { "3+ Minutes", "Icon_Time_3m" },
            { "5+ Minutes", "Icon_Time_5m" },
            { "7+ Minutes", "Icon_Time_7m" },
            { "10+ Minutes", "Icon_Time_10m" },
            { "15+ Minutes", "Icon_Time_15m" },
            { "20+ Minutes", "Icon_Time_20m" },
            { "30+ Minutes", "Icon_Time_30m" },
            { "45+ Minutes", "Icon_Time_45m" },
            { "1+ Hours", "Icon_Time_1h" },
            { "1.5+ Hours", "Icon_Time_1_5h" },
            { "2+ Hours", "Icon_Time_2h" },
            { "Timeless", "Icon_Time_Infinity" },

            // ===== Required Mods =====
            { "Youtube Stream", "Icon_Mod_Youtube" },
            { "Key Limiter", "Icon_Mod_KeyLimiter" },

            // ===== DLC =====
            { "DLC", "Icon_DLC_Base" },
            { "Hold", "Icon_DLC_Hold" },
            { "Multi Planet", "Icon_DLC_MultiPlanet" },

            // ===== Misc =====
            { "Pure Perfect Basescore Increase", "Icon_Misc_PurePerfect" },
            { "Auto Tile", "Icon_Misc_Auto" },
            { "Basescore Edit", "Icon_Misc_Basescore" },
        };
    }
}

