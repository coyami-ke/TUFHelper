using System;
using HarmonyLib;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using Together.Utils;
using UnityEngine;

namespace TUFHelper
{
    [HarmonyPatch]
    public static class PPDisplayerPatch
    {
        private static PPDisplayerScript ppDisplayer;
        private static GameObject ppDisplayerObject;
        private static PPDisplayerScript.Judgements judgements = new();

        static PPDisplayerPatch()
        {
            ADOFAIGameplayHandler.Editor_PlayButtonPressed += OnEditorPlayButtonPressed;
            ADOFAIGameplayHandler.Editor_Hit += OnEditorHit;
        }

        private static float speed => scnGame.instance.levelData.pitch / 100f * scnEditor.instance.playbackSpeed;
        private static int FloorCount => scrLevelMaker.instance.listFloors.Count - 1;
        private static ADOFAI.LevelData LevelData => scnGame.instance.levelData;

        private static void RegisterJudgement(HitMargin hit)
        {
            switch (hit)
            {
                case HitMargin.TooEarly: judgements.EarlyDouble++; break;
                case HitMargin.VeryEarly: judgements.EarlySingle++; break;
                case HitMargin.EarlyPerfect: judgements.EPerfect++; break;
                case HitMargin.Perfect: judgements.Perfect++; break;
                case HitMargin.LatePerfect: judgements.LPerfect++; break;
                case HitMargin.VeryLate: judgements.LateSingle++; break;
                case HitMargin.TooLate: judgements.LateDouble++; break;
                case HitMargin.FailMiss:
                case HitMargin.FailOverload: judgements.Deaths++; break;
            }
        }

        private static void LoadPPDisplayer()
        {
            const string assetPath = "assets/tufhelper/assets/prefabs/PPDisplayerPrefab.prefab";
            GameObject prefab = Main.assets.LoadAsset<GameObject>(assetPath);

            if (prefab == null)
            {
                Main.Logger.Error($"Failed to load prefab: {assetPath}");
                return;
            }

            ppDisplayerObject = GameObject.Instantiate(prefab);
            ppDisplayerObject.name = "TUFHelper_PPDisplayer";

            var canvas = GameObject.Find("Canvas")?.transform;
            if (canvas != null)
                ppDisplayerObject.transform.SetParent(canvas, false);

            var rect = ppDisplayerObject.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(-450, 0);
            rect.sizeDelta = new Vector2(857, 300);

            ppDisplayer = ppDisplayerObject.GetComponentInChildren<PPDisplayerScript>();
            PPDisplayerScript.FloorCount = FloorCount;
        }

        private static void UpdatePPDisplayer()
        {
            if (ppDisplayer == null) return;

            var account = AccountSaver.GetAccount();
            bool shouldDisplay = Main.Setting.ShowTUFHelperOverlayer &&  !(account?.IsRatingMode ?? false);
            bool flag2 = Main.Setting.ShowIngamePPCounter == false && Main.Setting.ShowIngameSpeed == false;
            ppDisplayerObject.SetActive(shouldDisplay || flag2);

            ppDisplayer.PP.gameObject.SetActive(Main.Setting.ShowIngamePPCounter);
            ppDisplayer.Speed.gameObject.SetActive(Main.Setting.ShowIngameSpeed);

            ppDisplayer.ApplySpped(speed);
            ppDisplayer.ApplyPP(0);

            if (Main.Setting.OverlayerElementsPositions.ContainsKey("PPDisplayer")) ppDisplayer.GetComponent<RectTransform>().localScale = new(Main.Setting.OverlayerElementsPositions["PPDisplayer"].Scale, Main.Setting.OverlayerElementsPositions["PPDisplayer"].Scale);
        }

        private static void OnEditorPlayButtonPressed(object sender, PlayButtonEventArgs e)
        {
            judgements.Reset();

            if (ppDisplayerObject == null)
            {
                LoadPPDisplayer();
            }

            UpdatePPDisplayer();
        }


        private static void OnEditorHit(object sender, HitMargin e)
        {
            if (ppDisplayer == null) return;

            RegisterJudgement(e);

            if (judgements.Deaths > 0)
            {
                ppDisplayer.ApplyPP(-1310);
                return;
            }

            float score = CalculateScore();
            ppDisplayer.ApplyPP(score);
        }

        private static float CalculateScore()
        {
            var passData = new PPDisplayerScript.PassData
            {
                IsNoHoldTap = Persistence.holdBehavior == HoldBehavior.NoHoldNeeded,
                Judgements = judgements,
                Speed = speed
            };

            var levelInfo = ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo;
            var diffName = DiffSpriteHelper.DiffIDRegister[levelInfo.DiffId];
            var diffScore = DiffSpriteHelper.DiffBaseScore[diffName];

            var levelData = new PPDisplayerScript.LevelData
            {
                BaseScore = levelInfo.BaseScore == 0 ? null : levelInfo.BaseScore,
                Difficulty = new PPDisplayerScript.Difficulty
                {
                    Name = diffName,
                    BaseScore = diffScore
                }
            };

            return (float)PPDisplayerScript.ScoreCalculator.GetScoreV2(passData, levelData);
        }
        [HarmonyPatch(typeof(scnEditor), "Start")]
        [HarmonyPostfix]
        public static void InitPPDisplayer()
        {
            // Force static constructor or call Init()
            _ = typeof(TUFHelper.PPDisplayerPatch); // triggers static constructor
        }
    }
}