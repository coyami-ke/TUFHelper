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
        public static PPDisplayerScript ppDisplayer { get; private set; }
        private static GameObject ppDisplayerObject;
        private static PPDisplayerScript.Judgements judgements = new();
        private const float ScoreUpdateInterval = 0.08f;
        private static float lastScoreUpdateTime = -999f;

        static PPDisplayerPatch()
        {
            ADOFAIGameplayHandler.Editor_PlayButtonPressed += OnEditorPlayButtonPressed;
            ADOFAIGameplayHandler.Editor_Hit += OnEditorHit;
            ADOFAIGameplayHandler.Editor_ScnGameTransferToEditor += Editor_ScnGameTransferToEditor;
        }

        private static void Editor_ScnGameTransferToEditor(object sender, ScnGameTransferToEditorEventArgs e)
        {
            if (ppDisplayer != null && ppDisplayer.gameObject != null)
            {
                ppDisplayer.gameObject.SetActive(false);
            }
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
            BundleFontFixer.FixFontsIn(ppDisplayerObject);
            ppDisplayerObject.name = "TUFHelper_PPDisplayer";

            var canvas = GameObject.Find("Canvas")?.transform;
            if (canvas != null)
                ppDisplayerObject.transform.SetParent(canvas, false);

            ppDisplayer = ppDisplayerObject.GetComponentInChildren<PPDisplayerScript>();
            PPDisplayerScript.FloorCount = FloorCount;
        }

        private static void UpdatePPDisplayer()
        {
            if (ppDisplayer == null) return;

            bool showInOverlayer = Main.Setting.ShowTUFHelperOverlayer;
            bool showIngameCounters = Main.Setting.ShowIngamePPCounter || Main.Setting.ShowIngameSpeed;
            bool isRatingPageActive = FrontPageScript.instance.IsRatingPageActive;
            bool show = showInOverlayer && showIngameCounters && !isRatingPageActive;

            ppDisplayerObject.SetActive(show);

            ppDisplayer.PP.gameObject.SetActive(Main.Setting.ShowIngamePPCounter);
            ppDisplayer.Speed.gameObject.SetActive(Main.Setting.ShowIngameSpeed);

            ppDisplayer.ApplySpped(speed);

            float configScale;
            if (Main.Setting.OverlayerElementsPositions.ContainsKey("PPDisplayer"))
            {
                configScale = Main.Setting.OverlayerElementsPositions["PPDisplayer"].Scale;
            }
            else configScale = 1;

            float canvasScaleFactor = 1f;
            Canvas rootCanvas = ppDisplayer.GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                canvasScaleFactor = rootCanvas.scaleFactor;
            }
            float finalScale = (1f / canvasScaleFactor) * configScale;

            ppDisplayer.GetComponent<RectTransform>().localScale = new Vector3(finalScale, finalScale, 1f);
        }

        private static void OnEditorPlayButtonPressed(object sender, PlayButtonEventArgs e)
        {
            judgements.Reset();
            lastScoreUpdateTime = -999f;

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

            float now = Time.unscaledTime;
            if (now - lastScoreUpdateTime < ScoreUpdateInterval)
            {
                return;
            }
            lastScoreUpdateTime = now;

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
            //if (levelInfo == null) return 0f;

            // ✅ Explicit Fallback Extraction: Solves type-mismatch and 0-score bugs
            //double calculatedBase = levelInfo.BaseScore ?? levelInfo.Difficulty?.BaseScore ?? 0.0;
            //double calculatedPPBase = levelInfo.PPBaseScore ?? calculatedBase;

            // ✅ Uses the standard initialization route for direct live gameplay
            var levelData = new PPDisplayerScript.LevelData(levelInfo);

            return (float)PPDisplayerScript.ScoreCalculator.GetScoreV2(passData, levelData);
        }

        [HarmonyPatch(typeof(scnEditor), "Start")]
        [HarmonyPostfix]
        public static void InitPPDisplayer()
        {
            _ = typeof(TUFHelper.PPDisplayerPatch);
        }
    }
}