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
    public class IngameLeaderboardPatch
    {
        public static bool IsInTUFHelper { get; set; }
        public static LevelListInfoElementJson LevelInfo { get; set; }

        [HarmonyPatch(typeof(scnEditor), "Start")]
        [HarmonyPrefix]
        public static void StartEditor()
        {
            if (!IsInTUFHelper) return;

            string assetName = "assets/tufhelper/assets/prefabs/ingameleaderboardprefab.prefab";

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
                Main.Logger.Log("Leaderboard prefab instantiated successfully.");
            }
        }

        [HarmonyPatch(typeof(scnGame), nameof(scnGame.instance.Play))]
        [HarmonyPrefix]
        public static async void Play()
        {
            if (!IsInTUFHelper) return;

            PassesListInfoElementJson[] passes = await GetPasses(LevelInfo.ID);
            
            if (IngameLeaderboardScript.instance != null)
            {
                IngameLeaderboardScript.instance.gameObject.SetActive(Main.Setting.ShowTUFHelperOverlayer);
                IngameLeaderboardScript.instance.StartCoroutine(
                    IngameLeaderboardScript.instance.LoadLeaderboardAsync(passes)
                );
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

        [HarmonyPatch(typeof(scrMistakesManager), nameof(scrMistakesManager.AddHit))]
        [HarmonyPostfix]
        public static void Postfix(HitMargin hit)
        {
            var player = IngameLeaderboardScript.PlayerRankPrefab.PassInfo;
            if (!IsInTUFHelper) return;
            switch (hit)
            {
                case HitMargin.TooEarly:
                    player.Judgements.EarlyDouble++;
                    break;
                case HitMargin.VeryEarly:
                    player.Judgements.EarlySingle++;
                    break;
                case HitMargin.EarlyPerfect:
                    player.Judgements.EPerfect++;
                    break;
                case HitMargin.Perfect:
                    player.Judgements.Perfect++;
                    break;
                case HitMargin.LatePerfect:
                    player.Judgements.LPerfect++;
                    break;
                case HitMargin.VeryLate:
                    player.Judgements.LateSingle++;
                    break;
                case HitMargin.TooLate:
                    player.Judgements.LateDouble++;
                    break;
                case HitMargin.FailMiss:
                    player.Judgements.Deaths++;
                    break;
                case HitMargin.FailOverload:
                    player.Judgements.Deaths++;
                    break;
            }
            bool flag = player.Judgements.Deaths > 0;
            if (!flag)
            {
                PPDisplayerScript.Judgements judg = new()
                {
                    Perfect = player.Judgements.Perfect,
                    Deaths = player.Judgements.Deaths,
                    EPerfect = player.Judgements.EPerfect,
                    LPerfect = player.Judgements.LPerfect,
                    EarlySingle = player.Judgements.EarlySingle,
                    LateSingle = player.Judgements.LateSingle,
                    EarlyDouble = player.Judgements.EarlyDouble,
                    LateDouble = player.Judgements.LateDouble,
                };

                var score = PPDisplayerScript.ScoreCalculator.GetScoreV2(new PPDisplayerScript.PassData
                {
                    IsNoHoldTap = Persistence.holdBehavior == HoldBehavior.NoHoldNeeded,
                    Judgements = judg,
                    Speed = scnGame.instance.levelData.pitch / 100f * scnEditor.instance.playbackSpeed
                }, new PPDisplayerScript.LevelData
                {
                    BaseScore = LevelInfo.BaseScore == 0 ? null : LevelInfo.BaseScore,
                    Difficulty = new PPDisplayerScript.Difficulty
                    {
                        Name = DiffSpriteHelper.DiffIDRegister[LevelInfo.DiffId],
                        BaseScore = DiffSpriteHelper.DiffBaseScore[DiffSpriteHelper.DiffIDRegister[LevelInfo.DiffId]]
                    }
                });

                //Main.Logger.Log($"Hold Behavior is: {scnEditor.instance.playbackSpeed} ae sPEED is: {(float)(leveldata.pitch/100)}");
                IngameLeaderboardScript.PlayerRankPrefab.PassInfo.ScoreV2 = (float)score;
                IngameLeaderboardScript.PlayerRankPrefab.PassInfo.Accuracy = (float)PPDisplayerScript.ScoreCalculator.GetXaccMtp(judg) / 10;
                IngameLeaderboardScript.PlayerRankPrefab.UpdateVisual();
                IngameLeaderboardScript.instance.UpdateRanks();
                
                // Main.Logger.Log(score.ToString());
            }
            else
            {
                //PPDisplayer.ApplyPP(-1310);
            }
        }
    }
}
