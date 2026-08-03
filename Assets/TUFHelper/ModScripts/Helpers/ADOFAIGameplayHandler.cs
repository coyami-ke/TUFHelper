using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TUFHelper.ModScripts.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace TUFHelper.Utils
{
    public class ScnGameTransferToEditorEventArgs : EventArgs
    {
        public bool IsFromTUFHelper { get; }
        public ScnGameTransferToEditorEventArgs(bool isFromTUFHelper)
        {
            IsFromTUFHelper = isFromTUFHelper;
        }
    }

    public class PlayButtonEventArgs : EventArgs
    {
        public LevelListInfoElementJson CurrentLevelInfo { get; }
        public bool RatingMode { get; }
        public bool IsFromTUFHelper { get; }
        public RatingElementJson CurrentRatingInfo { get; }
        public PlayButtonEventArgs(LevelListInfoElementJson level, bool isFromTUFHelper, bool ratingMode = false, RatingElementJson rating = null)
        {
            CurrentLevelInfo = level;
            IsFromTUFHelper = isFromTUFHelper;
            RatingMode = ratingMode;
            CurrentRatingInfo = rating;
        }
    }

    public class HitMarginEventArgs : EventArgs
    {
        public DetailedJudge DetailedJudge { get; }
        public HitMargin Hit { get; }
        public HitMarginEventArgs(DetailedJudge detailedJudge, HitMargin hit)
        {
            DetailedJudge = detailedJudge;
            Hit = hit;
        }
    }

    public enum DetailedJudge
    {
        None,
        XPerfect,
        PlusPerfect,
        MinusPerfect,
    }

    public static class ADOFAIGameplayHandler
    {
        public static event EventHandler<PlayButtonEventArgs> Editor_PlayButtonPressed;
        public static event EventHandler<HitMargin> Editor_Hit;
        public static event EventHandler<ScnGameTransferToEditorEventArgs> Editor_ScnGameTransferToEditor;
        public static event EventHandler<HitMarginEventArgs> Editor_HitMargin;

        public static void OpenLevel(string pathToLevel, LevelListInfoElementJson levelInfo)
        {
            HideUIFixPatch.RecentDirectLevelOpend = true;

            GCS.sceneToLoad = "scnEditor";
            GCS.worldEntrance = null;
            scnEditor.levelToOpenOnLoad = pathToLevel;

            IsFromTUFHelper = true;

            EditorPlayPatch.CurrentLevelInfo = levelInfo;

            //try
            //{
            //    var account = AccountSaver.GetAccount();
            //}
            //catch
            //{
            //    EditorPlayPatch.RatingMode = false;
            //    EditorPlayPatch.CurrentRating = null;
            //}

            SceneManager.LoadScene("scnEditor");
        }

        public static bool IsFromTUFHelper { get; set; }

        [HarmonyPatch(typeof(scrMarginTracker), nameof(scrMarginTracker.AddHit))]
        public static class AddHitPatch
        {
            public static void Prefix(HitMargin hit)
            {
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
                Editor_PlayButtonPressed?.Invoke(scnGame.instance, new(CurrentLevelInfo, IsFromTUFHelper, RatingMode, CurrentRating));
            }
        }

        [HarmonyPatch(typeof(scnEditor), "Update")]
        public static class ScnGameTransferToEditor
        {
            public static void Prefix()
            {
                if (Input.GetKeyDown(KeyCode.Escape) && scnEditor.instance.playMode)
                {
                    Editor_ScnGameTransferToEditor.Invoke(scnEditor.instance, new(IsFromTUFHelper));
                }
            }
        }

        [HarmonyPatch(typeof(scrMisc), "GetHitMargin")]
        public static class HitMarginPatch
        {
            private const double RadToDegMultiplier = 57.295780181884766;
            private const float RadToDegFloatMultiplier = 57.29578f;

            public static void Postfix(ref HitMargin __result, float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch)
            {
                try
                {
                    if (RDC.auto)
                    {
                        ADOFAIGameplayHandler.Editor_HitMargin?.Invoke(null, new(DetailedJudge.XPerfect, HitMargin.Perfect));
                        return;
                    }

                    if (__result != HitMargin.Perfect)
                    {
                        ADOFAIGameplayHandler.Editor_HitMargin?.Invoke(null, new(DetailedJudge.None, __result));
                        return;
                    }

                    

                    float rawDelta = (hitangle - refangle) * RadToDegFloatMultiplier;
                    float signedDeltaDeg = isCW ? rawDelta : -rawDelta;
                    float absoluteDeltaDeg = Mathf.Abs(signedDeltaDeg);

                    double radBoundary = scrMisc.TimeToAngleInRad(0.01667, (double)bpmTimesSpeed, (double)conductorPitch, false);
                    double val = radBoundary * RadToDegMultiplier;
                    double actualXPerfectBoundaryDeg = Math.Max(15.0, val);

                    DetailedJudge judge;
                    if ((double)absoluteDeltaDeg <= actualXPerfectBoundaryDeg)
                    {
                        judge = DetailedJudge.XPerfect;
                    }
                    else if (signedDeltaDeg < 0f)
                    {
                        judge = DetailedJudge.PlusPerfect;
                    }
                    else
                    {
                        judge = DetailedJudge.MinusPerfect;
                    }

                    ADOFAIGameplayHandler.Editor_HitMargin?.Invoke(null, new(judge, __result));
                }
                catch (Exception arg)
                {
                    Main.Logger.Log($"Error during self-contained XPerfect calculation: {arg}");
                    ADOFAIGameplayHandler.Editor_HitMargin?.Invoke(null, new(DetailedJudge.None, __result));
                }
            }
        }
    }
}