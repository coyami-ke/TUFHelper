using System;
using UnityEngine;

namespace TUFHelper.Utils
{
    public static class Helper
    {
        public static Sprite GetFlagSprite(string countryCode)
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

        public static string getDate(String UTCTime)
        {
            Debug.Log(UTCTime);
            DateTime utcTime = DateTime.Parse(UTCTime, null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime();

            TimeZoneInfo localTimeZone = TimeZoneInfo.Local;

            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, localTimeZone);

            return localTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static long GetTimeStamp(String UTCTime)
        {
            try
            {
                Debug.Log(UTCTime);
                DateTime utcTime = DateTime.Parse(UTCTime, null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime();
                long unixTimestamp = (long)(utcTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

                return unixTimestamp;
            } catch
            {
                Debug.Log(UTCTime);
                return 0;
            }

        }
    }
}