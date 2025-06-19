using System;
using System.Collections.Generic;
using System.Linq;

namespace TUFHelper.Utils
{
    public static class DiffSpriteHelper
    {
        public const string PATH_TO_DIFF_SPRITES = "Assets/TUFHelper/Assets/Sprites/DiffIcons/";
        public static readonly Dictionary<int, string> DiffIDRegister = new()
        {
            { 1004, "Gimmick" },
            { 10006123, "U13J" },
            { 1003, "Marathon" },
            { 10100, "Qq" },
            { 10002, "Q2" },
            { 10003, "Q2+" },
            { 10004, "Q3" },
            { 10005, "Q3+" },
            { 10006, "Q4" },
            { 10101, "-21"},
            { 1000, "-2" },
            { 0, "0" },
            { 1, "P1" },
            { 2, "P2" },
            { 3, "P3" },
            { 4, "P4" },
            { 5, "P5" },
            { 6, "P6" },
            { 7, "P7" },
            { 8, "P8" },
            { 9, "P9" },
            { 10, "P10" },
            { 11, "P11" },
            { 12, "P12" },
            { 13, "P13" },
            { 14, "P14" },
            { 15, "P15" },
            { 16, "P16" },
            { 17, "P17" },
            { 18, "P18" },
            { 19, "P19" },
            { 20, "P20" },
            { 21, "G1" },
            { 22, "G2" },
            { 23, "G3" },
            { 24, "G4" },
            { 25, "G5" },
            { 26, "G6" },
            { 27, "G7" },
            { 28, "G8" },
            { 29, "G9" },
            { 30, "G10" },
            { 31, "G11" },
            { 32, "G12" },
            { 33, "G13" },
            { 34, "G14" },
            { 35, "G15" },
            { 36, "G16" },
            { 37, "G17" },
            { 38, "G18" },
            { 39, "G19" },
            { 40, "G20" },
            { 41, "U1" },
            { 42, "U2" },
            { 43, "U3" },
            { 44, "U4" },
            { 45, "U5" },
            { 46, "U6" },
            { 47, "U7" },
            { 48, "U8" },
            { 49, "U9" },
            { 50, "U10" },
            { 51, "U11" },
            { 52, "U12" },
            { 53, "U13" },
            { 54, "U14" },
            { 55, "U15" },
            { 56, "U16" },
            { 57, "U17" },
            { 58, "U18" },
            { 59, "U19" },
            { 60, "U20" },
        };

        public static readonly Dictionary<string, double> DiffBaseScore = new()
        {
            { "Gimmick", 0},
            { "U13J", 0},
            { "Marathon" , 0 },
            { "Qq" , 0 },
            { "Q2" , 0 },
            { "Q2+" , 0 },
            { "Q3" , 0 },
            { "Q3+" , 0 },
            { "Q4" , 0 },
            { "-21" , 0},
            { "-2" , 0 },
            {"0" , 0},
            { "P1" , 0.1},
            { "P2" , 0.2 },
            { "P3" , 0.3 },
            { "P4" , 0.4 },
            { "P5" , 0.5 },
            { "P6" , 0.6 },
            { "P7" , 0.7 },
            { "P8" , 0.8 },
            { "P9" , 0.9 },
            { "P10" , 1 },
            { "P11" , 2 },
            { "P12" , 3 },
            { "P13" , 5 },
            { "P14" , 10 },
            { "P15" , 15 },
            { "P16" , 20 },
            { "P17" , 30 },
            { "P18" , 45 },
            { "P19" , 60 },
            { "P20" , 75 },
            { "G1" , 100 },
            { "G2" , 110 },
            { "G3" , 120 },
            { "G4" , 130 },
            { "G5" , 140 },
            { "G6" , 150 },
            { "G7" , 160 },
            { "G8" , 170 },
            { "G9" , 180 },
            { "G10" , 190 },
            { "G11" , 200 },
            { "G12" , 210 },
            { "G13" , 220 },
            { "G14" , 230 },
            { "G15" , 240 },
            { "G16" , 250 },
            { "G17" , 275 },
            { "G18" , 300 },
            { "G19" , 350 },
            { "G20" , 425 },
            { "U1" , 500 },
            { "U2" , 600 },
            { "U3" , 700 },
            { "U4" , 850 },
            { "U5" , 1000 },
            { "U6" , 1300 },   
            { "U7" , 1600 },   
            { "U8" , 1800 },
            { "U9" , 2000 },
            { "U10" , 2500 },
            { "U11" , 3000 },
            { "U12" , 4000 },
            { "U13" , 0 },
            { "U14" , 0 },
            { "U15" , 0 },
            { "U16" , 0 },
            { "U17" , 0 },
            { "U18" , 0 },
            { "U19" , 0 },
            { "U20" , 0 },
        };
        
        public static Dictionary<string, int> GetReversedDiffIDRegister()
        {
            Dictionary<string, int> dictionary = new();
            foreach (var keyValue in DiffIDRegister)
            {
                dictionary.Add(keyValue.Value, keyValue.Key);
            }
            return dictionary;
        }

        public static string GetSpriteFromId(int id) // i cant get the sprites for the Q diffs
        {
            if (DiffIDRegister.ContainsKey(id))
            {
                return $"{PATH_TO_DIFF_SPRITES}{DiffIDRegister[id]}.png";
            }
            else return $"{PATH_TO_DIFF_SPRITES}Unknown.png";
        }
        public static bool IsSpecialDiff(string diff)
        {
            return diff switch
            {
                "0" => true,
                "Gimmick" => true,
                "U13J" => true,
                "Marathon" => true,
                "-21" => true,
                "-2" => true,
                "Qq" => true,
                "Q2" => true,
                "Q2+" => true,
                "Q3" => true,
                "Q3+" => true,
                "Q4" => true,
                _ => false,
            };
        }
        public static bool IsSpecialDiff(int diff)
        {
            return diff switch
            {
                0 => true,
                1004 => true,
                10006123 => true,
                1003 => true,
                10100 => true,
                10002 => true,
                10003 => true,
                10004 => true,
                10005 => true,
                10006 => true,
                10101 => true,
                1000 => true,
                _ => false,
            };
            // { 1004, "Gimmick" },
            // { 10006123, "U13J" },
            // { 1003, "Marathon" },
            // { 10100, "Qq" },
            // { 10002, "Q2" },
            // { 10003, "Q2+" },
            // { 10004, "Q3" },
            // { 10005, "Q3+" },
            // { 10006, "Q4" },
            // { 10101, "-21"},
            // { 1000, "-2" },
        }
    }
}