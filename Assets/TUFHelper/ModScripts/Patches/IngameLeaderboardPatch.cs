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
        static IngameLeaderboardPatch()
        {
            // Register event handlers
            ADOFAIGameplayHandler.Editor_PlayButtonPressed += OnPlay;
            ADOFAIGameplayHandler.Editor_Hit += OnHit;
        }

        private static async void OnPlay(object sender, PlayButtonEventArgs e)
        {
            LevelListInfoElementJson levelInfo = e.CurrentLevelInfo;

            var passes = await GetPasses(levelInfo.ID);

            if (IngameLeaderboardScript.instance != null)
            {
                var account = AccountSaver.GetAccount();
                bool showInOverlayer = Main.Setting.ShowTUFHelperOverlayer;
                bool showIngameLeaderboard = Main.Setting.ShowIngameLeaderboard;
                bool isRatingPageActive = FrontPageScript.instance.IsRatingPageActive;
                bool show = showInOverlayer && showIngameLeaderboard && !isRatingPageActive;

                IngameLeaderboardScript.instance.gameObject.SetActive(show);

                IngameLeaderboardScript.instance.StartCoroutine(
                    IngameLeaderboardScript.instance.LoadLeaderboardAsync(passes)
                );

                if (Main.Setting.OverlayerElementsPositions.ContainsKey("IngameLeaderboard"))
                {
                    IngameLeaderboardScript.instance.GetComponent<RectTransform>().localScale =
                        new(Main.Setting.OverlayerElementsPositions["IngameLeaderboard"].Scale,
                            Main.Setting.OverlayerElementsPositions["IngameLeaderboard"].Scale);
                }

                // ✅ Reset player judgements to start fresh
                if (IngameLeaderboardScript.PlayerRankPrefab?.PassInfo?.Judgements != null)
                {
                    IngameLeaderboardScript.PlayerRankPrefab.PassInfo.Judgements = new();
                }
            }
        }


        private static void OnHit(object sender, HitMargin e)
        {
            if (IngameLeaderboardScript.PlayerRankPrefab == null) return;

            var player = IngameLeaderboardScript.PlayerRankPrefab.PassInfo;

            if (player.Judgements.Deaths > 0) return;

            var levelInfo = ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo;

            UpdateJudgements(player.Judgements, e);

            var judg = player.Judgements;

            var passData = new PPDisplayerScript.PassData
            {
                IsNoHoldTap = Persistence.holdBehavior == HoldBehavior.NoHoldNeeded,
                Judgements = new()
                {
                    Perfect = judg.Perfect,
                    LPerfect = judg.LPerfect,
                    EPerfect = judg.EPerfect,
                    EarlySingle = judg.EarlySingle,
                    LateSingle = judg.LateSingle,
                    EarlyDouble = judg.EarlyDouble,
                    LateDouble = judg.LateDouble,
                    Deaths = judg.Deaths
                },
                Speed = scnGame.instance.levelData.pitch / 100f * scnEditor.instance.playbackSpeed
            };


            string nameDiff; // = DiffSpriteHelper.DiffIDRegister[levelInfo.DiffId];

            try
            {
                nameDiff = DiffSpriteHelper.DiffIDRegister[levelInfo.DiffId]; 
            }
            catch
            {
                nameDiff = "0";
            }

            var levelData = new PPDisplayerScript.LevelData
            {
                Difficulty = new PPDisplayerScript.Difficulty
                {
                    Name = nameDiff,
                    BaseScore = levelInfo.Difficulty.BaseScore,
                    PPBaseScore = levelInfo.PPBaseScore ?? levelInfo.Difficulty.BaseScore
                }
            };

            player.ScoreV2 = (float)PPDisplayerScript.ScoreCalculator.GetScoreV2(passData, levelData);
            player.Accuracy = (float)PPDisplayerScript.ScoreCalculator.CalcAcc(judg);

            IngameLeaderboardScript.PlayerRankPrefab.UpdateVisual();
            IngameLeaderboardScript.instance.UpdateRanks();
        }



        private static void UpdateJudgements(PassesListInfoElementJudgementsJson judgements, HitMargin hit)
        {
            switch (hit)
            {
                case HitMargin.TooEarly:
                    judgements.EarlyDouble++; break;
                case HitMargin.VeryEarly:
                    judgements.EarlySingle++; break;
                case HitMargin.EarlyPerfect:
                    judgements.EPerfect++; break;
                case HitMargin.Perfect:
                    judgements.Perfect++; break;
                case HitMargin.LatePerfect:
                    judgements.LPerfect++; break;
                case HitMargin.VeryLate:
                    judgements.LateSingle++; break;
                case HitMargin.TooLate:
                    judgements.LateDouble++; break;
                case HitMargin.FailMiss:
                case HitMargin.FailOverload:
                    judgements.Deaths++; break;
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
                    webRequest.Abort();
                    return null;
                }
            }

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                return null;
            LevelListElementId levelDes = JsonConvert.DeserializeObject<LevelListElementId>(webRequest.downloadHandler.text);
            List<PassesListInfoElementJson> passes = levelDes.Level.Passes;
            if (passes == null) return Array.Empty<PassesListInfoElementJson>();
            return passes.OrderByDescending(p => p.ScoreV2).ToArray();
        }

        [HarmonyPatch(typeof(scnEditor), "Start")]
        [HarmonyPrefix]
        public static void StartEditor()
        {
            if (!ADOFAIGameplayHandler.IsFromTUFHelper) return;

            string assetName = "assets/tufhelper/assets/prefabs/ingameleaderboardprefab.prefab";
            GameObject prefab = Main.assets.LoadAsset<GameObject>(assetName);
            if (prefab != null && IngameLeaderboardScript.instance == null)
            {
                var obj = GameObject.Instantiate(prefab);
                Transform canvas = GameObject.Find("Canvas")?.transform;
                if (canvas != null)
                    obj.transform.SetParent(canvas, false);
                GameObject.DontDestroyOnLoad(obj);
            }
        }
    }
}
