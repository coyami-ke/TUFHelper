using HarmonyLib;
using System;
using System.IO;
using TUFHelper.ModScripts.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TUFHelper.Utils
{
    public class ScnGameTransferToEditorEventArgs : EventArgs
    {
        public bool IsFromTUFHelper { get; }
        public ScnGameTransferToEditorEventArgs(bool isFromTUFHelper) => IsFromTUFHelper = isFromTUFHelper;
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

    public static class ADOFAIGameplayHandler
    {
        public static event EventHandler<PlayButtonEventArgs> Editor_PlayButtonPressed;
        public static event EventHandler<HitMargin> Editor_Hit;
        public static event EventHandler<ScnGameTransferToEditorEventArgs> Editor_ScnGameTransferToEditor;

        public static string LastOpenedTUFLevel = "";
        public static bool IsFromTUFHelper { get; set; }

        /// <summary>
        /// Centralized level launch entry point.
        /// Handles UI state transitions, parameter tracking, and scene loading.
        /// </summary>
        public static void LaunchLevel(string pathToLevel, LevelListInfoElementJson levelInfo, string packId = null, int? packLevelId = null)
        {
            if (string.IsNullOrEmpty(pathToLevel))
            {
                Main.Logger.Error("[ADOFAIGameplayHandler] Attempted to launch a level with a null or empty path.");
                return;
            }

            LastOpenedTUFLevel = pathToLevel;
            IsFromTUFHelper = true;
            HideUIFixPatch.RecentDirectLevelOpend = true;

            // Update global UI state tracking
            if (!string.IsNullOrEmpty(packId))
            {
                FrontPageScript.IsPackListActive = true;
                FrontPageScript.LastOpenedPackId = packId;
                FrontPageScript.LastOpenedPackLevelId = packLevelId ?? -1;
            }
            else
            {
                FrontPageScript.IsPackListActive = false;
                FrontPageScript.LastOpenedPackId = string.Empty;
                FrontPageScript.LastOpenedPackLevelId = -1;
            }

            // Configure editor load targets
            GCS.sceneToLoad = "scnEditor";
            GCS.worldEntrance = null;
            scnEditor.levelToOpenOnLoad = pathToLevel;
            EditorPlayPatch.CurrentLevelInfo = levelInfo;

            // Execute scene transition with visual fade
            UIScript.SwipeToBlack(() => SceneManager.LoadScene("scnEditor"));
        }

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
                OpenLevelPatch.Postfix();
                Editor_PlayButtonPressed?.Invoke(scnGame.instance, new PlayButtonEventArgs(CurrentLevelInfo, IsFromTUFHelper, RatingMode, CurrentRating));
            }
        }

        [HarmonyPatch(typeof(scnEditor), "OpenLevelCo")]
        public static class OpenLevelPatch
        {
            private static string _targetLevelPath;

            public static void Prefix(string definedLevelPath)
            {
                _targetLevelPath = definedLevelPath;
            }

            public static void Postfix()
            {
                string currentPath = !string.IsNullOrEmpty(_targetLevelPath) ? _targetLevelPath : ADOBase.levelPath;

                if (string.IsNullOrEmpty(currentPath) || string.IsNullOrEmpty(LastOpenedTUFLevel))
                {
                    ResetTUFFlags();
                    return;
                }

                try
                {
                    string fullCurrent = Path.GetFullPath(currentPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    string fullLast = Path.GetFullPath(LastOpenedTUFLevel).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    if (!string.Equals(fullCurrent, fullLast, StringComparison.OrdinalIgnoreCase))
                    {
                        ResetTUFFlags();
                    }
                }
                catch (Exception ex)
                {
                    Main.Logger.Error($"[OpenLevelPatch] Failed to resolve level paths: {ex.Message}");
                    ResetTUFFlags();
                }
            }

            private static void ResetTUFFlags()
            {
                Main.isInTUFHelper = false;
                IsFromTUFHelper = false;
            }
        }

        [HarmonyPatch(typeof(scnEditor), "Update")]
        public static class ScnGameTransferToEditor
        {
            public static void Prefix()
            {
                if (Input.GetKeyDown(KeyCode.Escape) && scnEditor.instance.playMode)
                {
                    Editor_ScnGameTransferToEditor?.Invoke(scnEditor.instance, new ScnGameTransferToEditorEventArgs(IsFromTUFHelper));
                }
            }
        }
    }
}