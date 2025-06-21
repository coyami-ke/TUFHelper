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
using UnityEngine;
using UnityEngine.Networking;

namespace TUFHelper
{
    [HarmonyPatch]
    public class IngameLeaderboardPatch
    {
        public static int LevelID { get; set; }
        [HarmonyPatch(typeof(scnGame), nameof(scnGame.instance.Play))]
        [HarmonyPrefix]
        public static async void Play()
        {
            PassesListInfoElementJson[] passes = await GetPasses(LevelID);

            string assetName = "assets/tufhelper/assets/prefabs/ingameleaderboardprefab.prefab"; // Check with GetAllAssetNames

            GameObject prefab = Main.assets.LoadAsset<GameObject>(assetName);

            if (prefab != null && IngameLeaderboardScript.instance == null)
            {

                var obj = GameObject.Instantiate(prefab);

                Transform canvas = GameObject.Find("Canvas")?.transform;
                if (canvas != null)
                {
                    obj.transform.SetParent(canvas, false);
                }

                GameObject.DontDestroyOnLoad(obj);
                Main.Logger.Log("Prefab instantiated successfully.");

                var script = obj.GetComponent<IngameLeaderboardScript>();
                script.LoadLeaderboard(passes);
            }
            else
            {
                IngameLeaderboardScript.instance.LoadLeaderboard(passes);
                //IngameLeaderboardScript.instance.LoadLeaderboard(LoadedPasses);
            }
        }

        private static CancellationTokenSource currentRequestToken;
        public static async Task<PassesListInfoElementJson[]> GetPasses(int levelID)
        {
            currentRequestToken?.Cancel();
            currentRequestToken = new CancellationTokenSource();
            CancellationToken token = currentRequestToken.Token;

            string url = LeaderboardScript.GetDefaultUrl(levelID);
            using UnityWebRequest webRequest = UnityWebRequest.Get(url);
            webRequest.certificateHandler = new CertificateWhore();
            webRequest.timeout = 10;

            var operation = webRequest.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
                if (token.IsCancellationRequested)
                {
                    webRequest.Abort(); // Stop the request
                    return Array.Empty<PassesListInfoElementJson>();
                }
            }

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                return Array.Empty<PassesListInfoElementJson>();

            List<PassesListInfoElementJson> passes = JsonConvert.DeserializeObject<List<PassesListInfoElementJson>>(webRequest.downloadHandler.text);
            passes = passes.OrderByDescending(p => p.ScoreV2).ToList();

            return passes.ToArray();
        }
    }
}
