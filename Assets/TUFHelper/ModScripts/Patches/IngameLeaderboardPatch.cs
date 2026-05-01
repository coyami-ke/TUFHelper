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

                if (IngameLeaderboardScript.PlayerRankPrefab?.PassInfo?.Judgements != null)
                {
                    IngameLeaderboardScript.PlayerRankPrefab.PassInfo.Judgements = new();
                }
            }
        }


        private static readonly PPDisplayerScript.PassData _cachedPassData = new();
        private static readonly PPDisplayerScript.LevelData _cachedLevelData = new();

        private static void OnHit(object sender, HitMargin e)
        {
            var prefab = IngameLeaderboardScript.PlayerRankPrefab;
            if (prefab == null || IngameLeaderboardScript.instance == null) return;

            var player = prefab.PassInfo;
            if (player.Judgements.Deaths > 0) return;

            UpdateJudgements(player.Judgements, e);
            var judg = player.Judgements;

            _cachedPassData.IsNoHoldTap = Persistence.holdBehavior == HoldBehavior.NoHoldNeeded;
            _cachedPassData.Speed = scnGame.instance.levelData.pitch / 100f * scnEditor.instance.playbackSpeed;

            var pJudg = _cachedPassData.Judgements;
            pJudg.Perfect = judg.Perfect;
            pJudg.LPerfect = judg.LPerfect;
            pJudg.EPerfect = judg.EPerfect;
            pJudg.EarlySingle = judg.EarlySingle;
            pJudg.LateSingle = judg.LateSingle;
            pJudg.EarlyDouble = judg.EarlyDouble;
            pJudg.LateDouble = judg.LateDouble;
            pJudg.Deaths = judg.Deaths;

            var levelInfo = ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo;
            if (levelInfo == null) return;

            if (!DiffSpriteHelper.DiffIDRegister.TryGetValue(levelInfo.DiffId, out string nameDiff))
                nameDiff = "0";

            _cachedLevelData.BaseScore = levelInfo.Difficulty?.BaseScore;
            _cachedLevelData.PPBaseScore = levelInfo.PPBaseScore;

            _cachedLevelData.Difficulty.Name = nameDiff;
            _cachedLevelData.Difficulty.BaseScore = levelInfo.Difficulty?.BaseScore ?? 0.0;

            Main.Logger.Log(_cachedLevelData.GetBaseScore().ToString()); // 15, но счет все равно 0 поинтов

            player.ScoreV2 = (float)PPDisplayerScript.ScoreCalculator.GetScoreV2(_cachedPassData, _cachedLevelData);
            player.Accuracy = (float)PPDisplayerScript.ScoreCalculator.CalcAcc(judg);

            prefab.UpdateVisual();
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

            PassesListInfoElementJson[] levelDes = JsonConvert.DeserializeObject<PassesListInfoElementJson[]>(webRequest.downloadHandler.text);
            List<PassesListInfoElementJson> passes = levelDes.ToList();
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
