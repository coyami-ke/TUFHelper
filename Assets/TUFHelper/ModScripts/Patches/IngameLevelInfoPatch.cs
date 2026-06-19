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
            ADOFAIGameplayHandler.Editor_ScnGameTransferToEditor += ScnGameTransferToEditor;
        }

        private static void ScnGameTransferToEditor(object sender, ScnGameTransferToEditorEventArgs e)
        {
            if (IngameLevelInfoScript.Instance != null)
            {
                IngameLevelInfoScript.Instance.gameObject.SetActive(false);
            }
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

            var levelInfoScript = IngameLevelInfoScript.Instance;

            if (levelInfoScript == null)
            {
                var canvas = GameObject.Find("Canvas")?.transform;
                if (canvas != null)
                {
                    GameObject instance = GameObject.Instantiate(prefab);
                    BundleFontFixer.FixFontsIn(instance);
                    instance.transform.SetParent(canvas, false);

                    levelInfoScript = instance.GetComponent<IngameLevelInfoScript>();
                }
                else
                {
                    Main.Logger.Error("Canvas is null");
                    return;
                }
            }

            if (levelInfoScript == null)
            {
                Main.Logger.Error("IngameLevelInfoScript component could not be found or initialized.");
                return;
            }

            levelInfoScript.gameObject.SetActive(true);

            levelInfoScript.SetLevelInfo(e.CurrentLevelInfo);

            bool showInOverlayer = Main.Setting.ShowTUFHelperOverlayer;
            bool showIngameLevelInfo = Main.Setting.ShowIngameLevelInfo;

            bool isRatingPageActive = FrontPageScript.instance != null && FrontPageScript.instance.IsRatingPageActive;
            bool show = showInOverlayer && showIngameLevelInfo && !isRatingPageActive;

            float canvasScaleFactor = 1f;
            Canvas rootCanvas = levelInfoScript.GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                canvasScaleFactor = rootCanvas.scaleFactor;
            }

            float finalScale = 1f / canvasScaleFactor;
            levelInfoScript.GetComponent<RectTransform>().localScale = new Vector3(finalScale, finalScale, 1f);
            levelInfoScript.gameObject.SetActive(show);
        }

        [HarmonyPatch(typeof(scnEditor), "Start")]
        [HarmonyPostfix]
        public static void Init()
        {
            _ = typeof(IngameLevelInfoPatch); // triggers static constructor
        }
    }
}
