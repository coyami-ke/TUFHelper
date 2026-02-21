using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Newtonsoft.Json;
using Together.Utils;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace TUFHelper
{
    [HarmonyPatch]
    public class IngameLevelInfoPatch
    {
        static IngameLevelInfoPatch()
        {
            // Register event handlers
            ADOFAIGameplayHandler.Editor_PlayButtonPressed += OnPlay;
        }

        private static void OnPlay(object sender, PlayButtonEventArgs e)
        {
            const string assetPath = "assets/tufhelper/assets/prefabs/IngameLevelInfoPrefab.prefab";
            GameObject prefab = Main.assets.LoadAsset<GameObject>(assetPath);

            if (prefab == null)
            {
                Main.Logger.Error($"Failed to load prefab: {assetPath}");
                return;
            }

            var canvas = GameObject.Find("Canvas")?.transform;
            if (canvas != null)
            {
                if (IngameLevelInfoScript.Instance != null) return;
                GameObject instance = GameObject.Instantiate(prefab);
                instance.transform.SetParent(canvas, false);
                Main.Logger.Log("IngameLevelInfoPrefab instantiated.");
            }
            else
                Main.Logger.Error("Canvas is null");

            IngameLevelInfoScript.Instance.SetLevelInfo(e.CurrentLevelInfo);

            bool showInOverlayer = Main.Setting.ShowTUFHelperOverlayer;
            bool showIngameLevelInfo = Main.Setting.ShowIngameLevelInfo;
            bool isRatingPageActive = FrontPageScript.instance.IsRatingPageActive;
            bool show = showInOverlayer && showIngameLevelInfo && !isRatingPageActive;

            IngameLevelInfoScript.Instance.gameObject.SetActive(show);
        }

        [HarmonyPatch(typeof(scnEditor), "Start")]
        [HarmonyPostfix]
        public static void Init()
        {
            _ = typeof(IngameLevelInfoPatch); // triggers static constructor
        }
    }
}
