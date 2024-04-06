using System;
using UnityEngine;

namespace TUFHelper.Utils
{
    public static class Helper
    {
        public static double pguDiffToSortNumber(string pgu)
        {
            if (pgu.Equals("0.9"))
            {
                return 100;
            }
            else if (pgu.Equals("64"))
            {
                return 200;
            }
            else if (pgu.Equals("727"))
            {
                return 300;
            }
            else if (pgu.Equals("-22"))
            {
                return 400;
            }
            else if (pgu.Equals("-21"))
            {
                return 500;
            }
            else if (pgu.StartsWith("P"))
            {
                return double.Parse(pgu.Substring(1));
            }
            else if (pgu.StartsWith("G"))
            {
                return double.Parse(pgu.Substring(1)) + 20;
            }
            else if (pgu.StartsWith("U"))
            {
                return double.Parse(pgu.Substring(1)) + 40;
            }
            return 250;
        }

        public static string pguToLegacyDiff(string pgu)
        {
            string diffAsString;
            switch (pgu)
            {
                case "P1": diffAsString = "1"; break;
                case "P2": diffAsString = "3"; break;
                case "P3": diffAsString = "4"; break;
                case "P4": diffAsString = "5"; break;
                case "P5": diffAsString = "6"; break;
                case "P6": diffAsString = "7"; break;
                case "P7": diffAsString = "8"; break;
                case "P8": diffAsString = "9"; break;
                case "P9": diffAsString = "10"; break;
                case "P10": diffAsString = "11"; break;
                case "P11": diffAsString = "12"; break;
                case "P12": diffAsString = "13"; break;
                case "P13": diffAsString = "14"; break;
                case "P14": diffAsString = "15"; break;
                case "P15": diffAsString = "16"; break;
                case "P16": diffAsString = "17"; break;
                case "P17": diffAsString = "18"; break;
                case "P18": diffAsString = "18.5"; break;
                case "P19": diffAsString = "19"; break;
                case "P20": diffAsString = "19.5"; break;
                case "G1": diffAsString = "20.0"; break;
                case "G2": diffAsString = "20.05"; break;
                case "G3": diffAsString = "20.1"; break;
                case "G4": diffAsString = "20.15"; break;
                case "G5": diffAsString = "20.2"; break;
                case "G6": diffAsString = "20.25"; break;
                case "G7": diffAsString = "20.3"; break;
                case "G8": diffAsString = "20.35"; break;
                case "G9": diffAsString = "20.4"; break;
                case "G10": diffAsString = "20.45"; break;
                case "G11": diffAsString = "20.5"; break;
                case "G12": diffAsString = "20.55"; break;
                case "G13": diffAsString = "20.6"; break;
                case "G14": diffAsString = "20.65"; break;
                case "G15": diffAsString = "20.7"; break;
                case "G16": diffAsString = "20.75"; break;
                case "G17": diffAsString = "20.8"; break;
                case "G18": diffAsString = "20.85"; break;
                case "G19": diffAsString = "20.9"; break;
                case "G20": diffAsString = "20.95"; break;
                case "U1": diffAsString = "21"; break;
                case "U2": diffAsString = "21"; break;
                case "U3": diffAsString = "21.05"; break;
                case "U4": diffAsString = "21.05"; break;
                case "U5": diffAsString = "21.1"; break;
                case "U6": diffAsString = "21.1"; break;
                case "U7": diffAsString = "21.15"; break;
                case "U8": diffAsString = "21.15"; break;
                case "U9": diffAsString = "21.2"; break;
                case "U10": diffAsString = "21.2"; break;
                case "U11": diffAsString = "21.25"; break;
                case "U12": diffAsString = "21.25"; break;
                case "U13": diffAsString = "21.3"; break;
                case "U14": diffAsString = "21.3"; break;
                case "64": diffAsString = "64"; break;
                case "727": diffAsString = "727"; break;
                case "-22": diffAsString = "-22"; break;
                case "-21": diffAsString = "-21"; break;
                case "0.9": diffAsString = "0.9"; break;
                case "0": diffAsString = "0"; break;
                case "-2": diffAsString = "-2"; break;
                default: diffAsString = "unknown"; break;
            }
            return diffAsString;
        }

        public static Sprite getDiffSprite(string pgu)
        {
            if (Main.assets != null)
            {
                if (Main.Setting.showLegacyRating)
                {
                    return Main.assets.LoadAsset<Sprite>("Assets/TUFHelper/Assets/Sprites/DiffIcons/" + pguToLegacyDiff(pgu) + ".png");
                }
                else
                {
                    Sprite sprite = Main.assets.LoadAsset<Sprite>("Assets/TUFHelper/Assets/Sprites/DiffIcons/" + pgu + ".png");

                    if (sprite == null)
                    {
                        sprite = Main.assets.LoadAsset<Sprite>("Assets/TUFHelper/Assets/Sprites/DiffIcons/unknown.png");
                    }

                    return sprite;
                }
            }
            else
            {
                if (Main.Setting.showLegacyRating)
                {
                    return Resources.Load<Sprite>("DiffIcons/" + pguToLegacyDiff(pgu));
                }
                else
                {
                    Sprite sprite = Resources.Load<Sprite>("DiffIcons/" + pgu);

                    if (sprite == null)
                    {
                        sprite = Resources.Load<Sprite>("DiffIcons/unknown");
                    }

                    return sprite;
                }
            }
        }

        public static Sprite getFlagSprite(string countryCode)
        {
            if (Main.assets != null)
            {
                return Main.assets.LoadAsset<Sprite>("Assets/TUFHelper/Assets/Sprites/Flags/" + countryCode.ToLower() + ".png");
            }
            else
            {
                return Resources.Load<Sprite>("Flags/" + countryCode.ToLower());
            }
        }

        public static float calculatePercentXAcc(int[] hitss, int checkpoints)
        {
            int[] hits = new int[11];

            for (int i = 0;i < hitss.Length;i++)
            {
                hits[i] = hitss[i];
            }

            int total = 0;
            foreach (int num in hits)
            {
                total += num;
            }
            double num5 = (1.0 * (double)hits[3] + 1.0 * (double)hits[10] + 0.75 * (double)hits[2] + 0.75 * (double)hits[4] + 0.4 * (double)hits[1] + 0.4 * (double)hits[5] + 0.2 * (double)hits[0] + 0.2 * (double)hits[6]) / total;
            return (float)(num5 * Math.Pow(0.9875, checkpoints));
        }

        public static string getDate(String UTCTime)
        {
            Debug.Log(UTCTime);
            DateTime utcTime = DateTime.Parse(UTCTime, null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime();

            TimeZoneInfo localTimeZone = TimeZoneInfo.Local;

            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, localTimeZone);

            return localTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static long getTimeStamp(String UTCTime)
        {
            DateTime utcTime = DateTime.Parse(UTCTime, null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime();

            long unixTimestamp = (long)(utcTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            return unixTimestamp;
        }

    }


}