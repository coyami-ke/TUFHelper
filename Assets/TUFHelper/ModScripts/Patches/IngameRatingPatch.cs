using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using TMPro;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TUFHelper
{
    [HarmonyPatch]
    public static class IngameRatingPatch
    {
        static IngameRatingPatch()
        {
            ADOFAIGameplayHandler.Editor_PlayButtonPressed += OnPlay;
        }

        private static async void OnPlay(object sender, PlayButtonEventArgs e)
        {
            return; // disabled
            if (FrontPageScript.instance.IsRatingPageActive)
            {
                const string assetPath = "assets/tufhelper/assets/prefabs/IngameRatingPrefab.prefab";
                GameObject prefab = Main.assets.LoadAsset<GameObject>(assetPath);

                if (prefab == null)
                {
                    Main.Logger.Error($"Failed to load prefab: {assetPath}");
                    return;
                }

                var canvas = GameObject.Find("Canvas")?.transform;
                if (canvas != null)
                {
                    if (IngameRatingPanelScript.instance != null) return;
                    GameObject instance = GameObject.Instantiate(prefab);
                    instance.transform.SetParent(canvas, false);
                }
                else
                    Main.Logger.Error("Canvas is null");
                if (IngameRatingPanelScript.instance != null)
                {
                    await IngameRatingPanelScript.instance.SetRatingInfo(e.CurrentRatingInfo);
                }
            }
        }

        [HarmonyPatch(typeof(scnEditor), "Start")]
        [HarmonyPostfix]
        public static void Init()
        {
            //_ = typeof(IngameRatingPatch); // triggers static constructor
        }

        // [HarmonyPatch(typeof(scrEnableIfBeta), "Awake")]
        // [HarmonyPostfix]
        // public static void PatchHUD()
        // {
        //     var txt = GameObject.Find("txtBetaIndicator")?.transform;
        //     if (txt != null)
        //     {
        //         txt.GetComponent<TMP_Text>().text = "tung tung tung sahur";
        //     }
        // }
    }
}