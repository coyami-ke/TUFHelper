using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using TUFHelper.ModScripts.Helpers;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TUFHelper

{
    public static class MoveToLobbyPatch
    {
        public static void MoveToTUFMenu()
        {
            GCS.sceneToLoad = "";
            scrUIController.instance.WipeToBlack(WipeDirection.StartsFromRight, () =>
            {
                Main.isInTUFHelper = true;
                string scenePath = Main.scenes.GetAllScenePaths().FirstOrDefault(p => p.EndsWith("TUFLevelSelect.unity"));
                if (!string.IsNullOrEmpty(scenePath))
                {
                    SceneManager.LoadScene(scenePath);
                    DiscordController.shouldUpdatePresence = true;
                }
                else
                {
                    Main.Logger.Error("Scene 'TUFLevelSelect' not found in AssetBundle. :sob:");
                }
            });
        }


        [HarmonyPatch]
        public static class TogetherEnter
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(scnLevelSelect), "Update", (Type[])null, (Type[])null);
                yield return AccessTools.Method(typeof(scnLevelSelectTaro), "Update", (Type[])null, (Type[])null);
            }

            public static void Postfix()
            {
                Postfix_();
            }

            private static void Postfix_()
            {
                if (RDEditorUtils.CheckForKeyCombo(true, true, KeyCode.U)) // hotkey
                {
                    GCS.sceneToLoad = "";
                    scrUIController.instance.WipeToBlack(WipeDirection.StartsFromRight, () =>
                    {
                        Main.isInTUFHelper = true;
                        MoveToTUFMenu();// SceneManager.LoadScene("Assets/TUFHelper/Scenes/TUFLevelSelect.unity");
                    });
                }
            }
        }
        [HarmonyPatch]
        public static class LoadTUFHelper
        {
            private static bool isFirst = true;
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(scnLevelSelect), "Start", (Type[])null, (Type[])null);
            }
            public static void Postfix()
            {
                if (isFirst && Main.Setting.StartWithGame)
                {
                    GCS.sceneToLoad = "";
                    scrUIController.instance.WipeToBlack(WipeDirection.StartsFromRight, () =>
                    {
                        Main.isInTUFHelper = true;
                        MoveToTUFMenu(); //SceneManager.LoadScene("Assets/TUFHelper/Scenes/TUFLevelSelect.unity");
                    });
                    isFirst = false;
                }
            }
        }

        [HarmonyPatch(typeof(scrController), "OnLandOnPortal")]
        public static class OnLandOnPortal
        {
            public static bool Prefix(Portal portalDestination, string portalArguments)
            {
                return Prefix_(portalDestination, portalArguments);
            }

            private static bool Prefix_(Portal portalDestination, string portalArguments)
            {
                if (portalArguments != null && portalArguments.Equals("TUFHelperPortalFloor"))
                {
                    GCS.sceneToLoad = "";
                    
                    scrUIController.instance.WipeToBlack(WipeDirection.StartsFromRight, () =>
                    {
                        Main.isInTUFHelper = true;
                        MoveToTUFMenu(); //SceneManager.LoadScene("Assets/TUFHelper/Scenes/TUFLevelSelect.unity");
                    });
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PauseMenu), "Choose")]
        public static class Choose_Patch
        {
            public static bool Prefix(PauseMenu __instance, int ___selectedIndex)
            {
                if (Main.isInTUFHelper)
                {
                    PauseMenu.ButtonType bt = (__instance.currentButtons[___selectedIndex] as PauseButton).buttonType;
                    if (bt.ToString().Equals("Quit"))
                    {
   
                        Time.timeScale = 1;
                        GCS.sceneToLoad = "Assets/TUFHelper/Scenes/TUFLevelSelect.unity";
                        // IngameLeaderboardPatch.IsInTUFHelper = false;
                        // PPDisplayerPatch.IsFromTUFH = false;
                        ADOFAIGameplayHandler.IsFromTUFHelper = false;
                        scrUIController.instance.WipeToBlack(WipeDirection.StartsFromRight, null);
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(scnEditor), "QuitToMenu")]
        public static class QuitToMenu_Patch
        {
            public static bool Prefix()
            {
                if (Main.isInTUFHelper)
                {
                    Time.timeScale = 1;
                    GCS.sceneToLoad = "Assets/TUFHelper/Scenes/TUFLevelSelect.unity";
                    // IngameLeaderboardPatch.IsInTUFHelper = false;
                    // PPDisplayerPatch.IsFromTUFH = false;
                    ADOFAIGameplayHandler.IsFromTUFHelper = false;
                    scrUIController.instance.WipeToBlack(WipeDirection.StartsFromRight, null);
                    return false;
                }
                return true;
            }
        }
        
        
        private static readonly List<GameObject> portals = new List<GameObject>();
        private static readonly List<Transform> transforms = new List<Transform>();

        [HarmonyPatch(typeof(scnLevelSelect), "Start")]
        public static class AddPortal
        {
            public static void Postfix()
            {
                AddMainMenuPortal();
            }
        }

        internal static void AddMainMenuPortal()
        {
            foreach (GameObject o in portals)
            {
                if (o != null)
                {
                    GameObject.Destroy(o);
                }
            }
            foreach (Transform t in transforms)
            {
                if (t != null)
                {
                    GameObject.Destroy(t);
                }
            }
            portals.Clear();
            transforms.Clear();

            GameObject ring = GameObject.Find("outer ring");
            GameObject portal = GameObject.Instantiate<GameObject>(ring.transform.Find("FloorCalibration").gameObject, ring.transform);
            portal.name = "TUFHelperPortal";
            scrFloor floor = portal.GetComponent<scrFloor>();
            floor.levelnumber = Portal.None;
            floor.arguments = "TUFHelperPortalFloor";
            portal.transform.position = new Vector3(3f, 1f);
            GameObject canvasWorld = GameObject.Find("Canvas World");

            Transform transform = GameObject.Instantiate<Transform>(canvasWorld.transform.Find("Calibration"), canvasWorld.transform);
            transform.position = new Vector3(5.4f, 1f);
            transform.name = "TUFHelperPortalText";
            transform.GetComponent<scrTextChanger>().desktopText = "TUF Levels";
            transform.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            RDExtensions.ScaleXY(transform, 0.6f, 0.6f);

            portals.Add(portal);
            transforms.Add(transform);
        }
    }
}
