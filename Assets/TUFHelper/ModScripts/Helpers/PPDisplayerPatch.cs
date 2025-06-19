using System.Collections.Generic;
using HarmonyLib;
using OggVorbisEncoder.Setup;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;

namespace TUFHelper
{
    public class PPDisplayerPatch
    {
        public static LevelListInfoElementJson Levelinfo;
        static PPDisplayerScript PPDisplayer;
        static PPDisplayerScript.Judgements judgements = new();
        static GameObject text;
        static float speed;
        static bool _isFromTUFH = false;
        public static bool IsFromTUFH
        {
            get => _isFromTUFH;
            set
            {
                if (_isFromTUFH == value) return;
                _isFromTUFH = value;

                if (!value)
                {
                    if (text != null)
                    {
                        text.SetActive(false);
                    }
                }
            }
        }
        private static int FloorCount
        {
            get
            {
                return scrLevelMaker.instance.listFloors.Count - 1;
            }
        }
        static ADOFAI.LevelData leveldata
        {
            get
            {
                return scnGame.instance.levelData;
            }
        }

        static bool holdingcontrol
        {
			get { return RDInput.holdingControl; }
        }

        [HarmonyPatch(typeof(scnEditor), "ResetScene")]
        internal static class scnEditor_ResetScene_Patch
        {
            private static void Postfix()
            {
                if (!IsFromTUFH) return;
                if (text != null)
                {
                    text.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(scrController), "StartLoadingScene")]
        internal static class scrController_StartLoadingScene_Patch
        {
            private static void Postfix()
            {
                if (!IsFromTUFH) return;
                if (text != null)
                {
                    text.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(scnGame), nameof(scnGame.instance.Play))]
        public static class EditorPlayPatch
        {
            public static void Prefix()
            {
                if (!IsFromTUFH) return;
                if (judgements == null)
                {
                    Main.Logger.Log("Jugement nulls!! Dies");
                    return;
                }
                judgements.Reset();
                string assetName = "assets/tufhelper/assets/prefabs/PPDisplayerPrefab.prefab"; // Check with GetAllAssetNames
                speed = (float)((leveldata.pitch / 100f) * scnEditor.instance.playbackSpeed);

                if (text == null)
                {
                    // Creat Prefab
                    GameObject prefab = Main.assets.LoadAsset<GameObject>(assetName);
                    if (prefab != null)
                    {
                        text = GameObject.Instantiate(prefab);
                        Transform canvas = GameObject.Find("Canvas")?.transform;
                        if (canvas != null)
                            text.transform.SetParent(canvas, false);

                        var rt = text.GetComponent<RectTransform>();
                        rt.anchoredPosition = new Vector2(-450, 0);
                        rt.sizeDelta = new Vector2(857, 300);
                        Main.Logger.Log("Prefab instantiated successfully.");

                        PPDisplayer = text.transform.GetComponentInChildren<PPDisplayerScript>();
                        PPDisplayer.ApplySpped(speed);
                        PPDisplayer.ApplyPP(0);
                        PPDisplayerScript.FloorCount = FloorCount;
                    }
                    else
                    {
                        Main.Logger.Error($"Failed to load prefab from AssetBundle. ({assetName})");
                    }
                }
                else
                {
                    text.SetActive(true);
                    PPDisplayer.ApplySpped(speed);
                    PPDisplayer.ApplyPP(0);
                }
                // Safety Measure so that the EXACT level must be played to calc score, only works when manually loading a new level tho!
                if (PPDisplayerScript.currentPathdata != leveldata.pathData && PPDisplayerScript.currentAnglePath != leveldata.angleData) PPDisplayerPatch.IsFromTUFH = false;
            }
        }
        [HarmonyPatch(typeof(scrMistakesManager), nameof(scrMistakesManager.AddHit))]
        public static class AddHitPatch
        {
            // Checking Judgement each hit
            public static void Postfix(HitMargin hit)
            {
                if (!IsFromTUFH) return;
                if (judgements == null)
                {
                    Main.Logger.Log("Judgement is Nul!!!!");
                    return;
                }
                switch (hit)
                {
                    case HitMargin.TooEarly:
                        judgements.EarlyDouble++;
                        break;
                    case HitMargin.VeryEarly:
                        judgements.EarlySingle++;
                        break;
                    case HitMargin.EarlyPerfect:
                        judgements.EPerfect++;
                        break;
                    case HitMargin.Perfect:
                        judgements.Perfect++;
                        break;
                    case HitMargin.LatePerfect:
                        judgements.LPerfect++;
                        break;
                    case HitMargin.VeryLate:
                        judgements.LateSingle++;
                        break;
                    case HitMargin.TooLate:
                        judgements.LateDouble++;
                        break;
                    case HitMargin.FailMiss:
                        judgements.Deaths++;
                        break;
                    case HitMargin.FailOverload:
                        judgements.Deaths++;
                        break;
                }
                bool flag = judgements.Deaths > 0;
                if (!flag)
                {
                    var score = PPDisplayerScript.ScoreCalculator.GetScoreV2(new PPDisplayerScript.PassData
                    {
                        IsNoHoldTap = Persistence.holdBehavior == HoldBehavior.NoHoldNeeded,
                        Judgements = judgements,
                        Speed = speed
                    }, new PPDisplayerScript.LevelData
                    {
                        BaseScore = (Levelinfo.BaseScore == 0 ? null : Levelinfo.BaseScore),
                        Difficulty = new PPDisplayerScript.Difficulty
                        {
                            Name = DiffSpriteHelper.DiffIDRegister[Levelinfo.DiffId],
                            BaseScore = DiffSpriteHelper.DiffBaseScore[DiffSpriteHelper.DiffIDRegister[Levelinfo.DiffId]]
                        }
                    });

                    //Main.Logger.Log($"Hold Behavior is: {scnEditor.instance.playbackSpeed} ae sPEED is: {(float)(leveldata.pitch/100)}");
                    PPDisplayer.ApplyPP(score);
                    Main.Logger.Log(score.ToString());
                }
                else
                {
                    PPDisplayer.ApplyPP(-1310);
                }
            }
        }
    }
}

