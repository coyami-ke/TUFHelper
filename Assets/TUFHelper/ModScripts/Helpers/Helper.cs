using System;
using UnityEngine;

namespace TUFHelper.Utils
{
    using System;

    public static class RelativeTimeFormatter
    {
        private const int SECOND = 1;
        private const int MINUTE = 60 * SECOND;
        private const int HOUR = 60 * MINUTE;
        private const int DAY = 24 * HOUR;
        private const int MONTH = 30 * DAY;

        /// <summary>
        /// Formats a DateTime into a relative string (e.g., "2 hours ago", "Yesterday", "3 months ago").
        /// Accepts UTC, Local, or Unspecified timestamps safely.
        /// </summary>
        public static string ToRelativeTime(this DateTime dateTime)
        {
            // Convert input time to UTC for an accurate comparison against DateTime.UtcNow
            DateTime utcTime = dateTime.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                _ => dateTime
            };

            TimeSpan timeSpan = DateTime.UtcNow - utcTime;
            double delta = timeSpan.TotalSeconds;

            // Future dates or clock desync edge cases
            if (delta < 0)
            {
                return "just now";
            }

            if (delta < 1 * MINUTE)
            {
                return timeSpan.Seconds <= 5 ? "just now" : $"{timeSpan.Seconds} seconds ago";
            }

            if (delta < 2 * MINUTE)
            {
                return "1 minute ago";
            }

            if (delta < 45 * MINUTE)
            {
                return $"{timeSpan.Minutes} minutes ago";
            }

            if (delta < 90 * MINUTE)
            {
                return "1 hour ago";
            }

            if (delta < 24 * HOUR)
            {
                return $"{timeSpan.Hours} hours ago";
            }

            if (delta < 48 * HOUR)
            {
                return "yesterday";
            }

            if (delta < 30 * DAY)
            {
                return $"{timeSpan.Days} days ago";
            }

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 30));
                return months <= 1 ? "1 month ago" : $"{months} months ago";
            }

            int years = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 365));
            return years <= 1 ? "1 year ago" : $"{years} years ago";
        }

        /// <summary>
        /// Overload for parsing API string timestamps (ISO 8601 / standard date strings).
        /// Returns the original string fallback if parsing fails.
        /// </summary>
        public static string ToRelativeTime(string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime parsedDate))
            {
                return parsedDate.ToRelativeTime();
            }

            return dateString; // Fallback to raw string if format is invalid
        }
    }
    public static class VectorExtensions
    {
        /// <summary>
        /// Converts a System.Numerics.Vector2 to a UnityEngine.Vector2
        /// </summary>
        public static UnityEngine.Vector2 ToUnity(this System.Numerics.Vector2 vector)
        {
            return new UnityEngine.Vector2(vector.X, vector.Y);
        }

        /// <summary>
        /// Converts a UnityEngine.Vector2 to a System.Numerics.Vector2
        /// </summary>
        public static System.Numerics.Vector2 ToSystem(this UnityEngine.Vector2 vector)
        {
            return new System.Numerics.Vector2(vector.x, vector.y);
        }
    }
    public static class Helper
    {
        public static Sprite GetFlagSprite(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                return null;
            }

            countryCode = countryCode.Trim().ToLowerInvariant();
            if (countryCode is "xx" or "unknown" or "null")
            {
                return null;
            }

            if (Main.assets != null)
            {
                return Main.GetSpriteFromAssets("Assets/TUFHelper/Assets/Sprites/Flags/" + countryCode + ".png");
            }
            else
            {
                return Resources.Load<Sprite>("Flags/" + countryCode);
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
