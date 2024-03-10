using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ADOFAI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;


namespace TUFHelper
{
    internal class LevelLoadPatch
    {
        public static string SongPath;
        
        private static bool isExecute;
        private static bool isCustomError;
        
        public static bool IsLoadDirectLevel;
        public static bool IsLoading;
        
        public static List<string> RemoveLevels = new List<string>();
        
        
        [HarmonyPatch(typeof(scnCLS), "Awake")]
        internal static class Fixbug
        {
            internal static void Prefix()
            {
                
                if (!IsLoadDirectLevel) return;
                GCS.customLevelPaths = null;
                GCS.customLevelId = null;
                IsLoadDirectLevel = false;
            }
        }
        
        [HarmonyPatch(typeof(scnLevelSelect), "Awake")]
        private static class OnAodfaiLoaded
        {
            private static void Prefix()
            {
                if (isExecute) return;
                isExecute = true;

                Application.wantsToQuit += () =>
                {
                    foreach (var levelPath in RemoveLevels)
                        Directory.Delete(levelPath, true);
                    return true;
                };

            }
        }
        
        
        [HarmonyPatch(typeof(scnGame), "LoadLevel")]
        private static class FixSongBug
        {
            private static void Postfix(scnGame __instance)
            {
                if (IsLoading)
                {
                    if (!string.IsNullOrEmpty(SongPath))
                    {
                        __instance.levelData.songFilename = Path.GetFileName(SongPath);
                        __instance.levelPath = SongPath.Replace(Path.GetFileName(SongPath), string.Empty);
                    }

                    SongPath = null;

                    IsLoading = false;
                }
            }
        }

        
        [HarmonyPatch(typeof(scrController), "Hit")]
        private static class DisableHit
        {
            private static bool Prefix()
            {
                return !IsLoading;
            }
        }
        
        
    }
}