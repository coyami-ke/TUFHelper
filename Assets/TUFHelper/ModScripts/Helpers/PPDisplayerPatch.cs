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
                return scnEditor.instance.levelData;
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
                string assetName = "assets/tufhelper/assets/prefabs/PPDisplayer.prefab"; // Check with GetAllAssetNames
                speed = (leveldata.pitch / 100) + (holdingcontrol ? scnEditor.instance.playbackSpeed : 0);

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

                    Main.Logger.Log($"Hold Behavior is: {Persistence.holdBehavior}");
                    Main.Logger.Log($"Judgements are: {judgements.Perfect}");
                    Main.Logger.Log($"BaseScore passed: {Levelinfo.BaseScore}, DiffScore: {DiffSpriteHelper.DiffBaseScore[DiffSpriteHelper.DiffIDRegister[Levelinfo.DiffId]]}");
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

