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
        public PlayButtonEventArgs(LevelListInfoElementJson level)
        {
            CurrentLevelInfo = level;
        }
    }
    public class HitEventArgs : EventArgs
    {
        public HitMargin Hit { get; }
        public HitEventArgs(HitMargin hit)
        {
            Hit = hit;
        }
    }
    public static class ADOFAIGameplayHandler
    {
        public static event EventHandler<PlayButtonEventArgs> Editor_PlayButtonPressed;
        public static event EventHandler<HitEventArgs> Editor_Hit;

        public static bool IsFromTUFHelper { get; set; }

        [HarmonyPatch(typeof(scrMistakesManager), nameof(scrMistakesManager.AddHit))]
        public static class AddHitPatch
        {
            public static void Postfix(HitMargin hit)
            {
                if (!IsFromTUFHelper) return;
                Editor_Hit?.Invoke(null, new(hit));
            }
        }

        [HarmonyPatch(typeof(scnGame), nameof(scnGame.instance.Play))]
        public static class EditorPlayPatch
        {
            public static LevelListInfoElementJson CurrentLevelInfo { get; set; }
            public static void Postfix()
            {
                if (!IsFromTUFHelper) return;
                Editor_PlayButtonPressed?.Invoke(scnGame.instance, new(CurrentLevelInfo));
            }
        }
    }
}