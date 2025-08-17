using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using HarmonyLib;
using TUFHelper.ModScripts.Json;
using UnityEditor;
using UnityEngine;

namespace TUFHelper.Utils
{
    public class PlayButtonEventArgs : EventArgs
    {
        public LevelListInfoElementJson CurrentLevelInfo { get; }
        public bool RatingMode { get; }
        public RatingElementJson CurrentRatingInfo { get; }
        public PlayButtonEventArgs(LevelListInfoElementJson level, bool ratingMode = false, RatingElementJson rating = null)
        {
            CurrentLevelInfo = level;
            RatingMode = ratingMode;
            CurrentRatingInfo = rating;
        }
    }
    public static class ADOFAIGameplayHandler
    {
        public static event EventHandler<PlayButtonEventArgs> Editor_PlayButtonPressed;
        public static event EventHandler<HitMargin> Editor_Hit;

        public static bool IsFromTUFHelper { get; set; }

        [HarmonyPatch(typeof(scrMistakesManager), nameof(scrMistakesManager.AddHit))]
        public static class AddHitPatch
        {
            public static void Prefix(HitMargin hit)
            {
                if (!IsFromTUFHelper || !Main.Setting.ShowTUFHelperOverlayer) return;
                Editor_Hit?.Invoke(null, hit);
            }
        }

        [HarmonyPatch(typeof(scnGame), nameof(scnGame.instance.Play))]
        public static class EditorPlayPatch
        {
            public static LevelListInfoElementJson CurrentLevelInfo { get; set; }
            public static RatingElementJson CurrentRating { get; set; }
            public static bool RatingMode { get; set; }
            public static void Prefix()
            {
                if (!IsFromTUFHelper) return;
                Editor_PlayButtonPressed?.Invoke(scnGame.instance, new(CurrentLevelInfo, RatingMode, CurrentRating));
            }
        }
    }
}