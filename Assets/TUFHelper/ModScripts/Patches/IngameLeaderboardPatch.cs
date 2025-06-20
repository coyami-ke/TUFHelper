using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace TUFHelper
{
    [HarmonyPatch]
    public class ScnGamePatch
    {
        [HarmonyPatch(typeof(scnGame), nameof(scnGame.instance.Play))]
        [HarmonyPrefix]
        public static void Play()
        {
            string assetName = "assets/tufhelper/assets/prefabs/ingameleaderboardprefab.prefab"; // Check with GetAllAssetNames

            GameObject prefab = Main.assets.LoadAsset<GameObject>(assetName);

            if (prefab != null)
            {
                var obj = GameObject.Instantiate(prefab);

                Transform canvas = GameObject.Find("Canvas")?.transform;
                if (canvas != null)
                {
                    obj.transform.SetParent(canvas, false);
                }

                GameObject.DontDestroyOnLoad(obj);
                Main.Logger.Log("Prefab instantiated successfully.");
            }
            else
            {
                Main.Logger.Error($"Failed to load prefab from AssetBundle. ({assetName})");
            }
        }
    }
}
