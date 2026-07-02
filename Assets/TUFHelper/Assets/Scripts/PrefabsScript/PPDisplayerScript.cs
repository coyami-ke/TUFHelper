using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;

[RegisterIngameElement("PPDisplayer", "assets/tufhelper/assets/prefabs/PPDisplayerPrefab.prefab")]
public class PPDisplayerScript : BasicIngameElement
{
    public TextMeshProUGUI PP, Speed;
    public LevelListInfoElementJson Levelinfo;

    private double curScore;
    private Tween _scoreTween;

    public static List<float> currentAnglePath = new();
    public static string currentPathdata;
    public static int FloorCount;

    // Runtime state tracking cache
    private readonly Judgements judgements = new();
    private float lastScoreUpdateTime = -999f;
    private const float ScoreUpdateInterval = 0.08f;
    private LevelData _cachedLevelData;

    private PPDisplayerSettingsCategory displayerSettings;

    public override string ID => "PPDisplayer";
    public override string NameInSettings => "Score & Speed";
    public override Sprite Icon => Main.assets.LoadAsset<Sprite>("assets/tufhelper/assets/sprites/ppdisplayer.png");

    private static ADOFAI.LevelData leveldata
    {
        get => scnGame.instance != null ? scnGame.instance.levelData : null;
    }

    private float CurrentLevelSpeed =>
        (scnGame.instance != null && scnEditor.instance != null)
            ? scnGame.instance.levelData.pitch / 100f * scnEditor.instance.playbackSpeed
            : 1.0f;

    private int CurrentFloorCount =>
        (scrLevelMaker.instance != null && scrLevelMaker.instance.listFloors != null)
            ? scrLevelMaker.instance.listFloors.Count - 1
            : 0;

    private void Awake()
    {
        if (PP != null) PP.text = string.Empty;
        if (Speed != null) Speed.text = string.Empty;

        if (scnGame.instance != null && leveldata != null)
        {
            currentAnglePath = leveldata.angleData ?? new List<float>();
            currentPathdata = leveldata.pathData ?? string.Empty;
        }
        else
        {
            currentAnglePath = new List<float>();
            currentPathdata = string.Empty;
        }
    }

    public override void OnSettingsOpened()
    {
        ApplyPP(727.7f);
        ApplySpped(1.0f);
    }

    protected override void Start()
    {
        base.Start();

        if (PP != null && Main.Setting != null)
            PP.gameObject.SetActive(Main.Setting.ShowIngamePPCounter);

        if (Speed != null && Main.Setting != null)
            Speed.gameObject.SetActive(Main.Setting.ShowIngameSpeed);
    }

    protected override bool ShouldElementBeVisible()
    {
        if (Main.Setting == null) return true;
        return Main.Setting.ShowIngamePPCounter || Main.Setting.ShowIngameSpeed;
    }

    #region Self-Contained Gameplay Event Hooks

    protected override void OnPlay(PlayButtonEventArgs e)
    {
        judgements.Reset();
        lastScoreUpdateTime = -999f;
        FloorCount = CurrentFloorCount;

        ApplySpped(CurrentLevelSpeed);

        if (ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo != null)
        {
            var levelInfo = ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo;
            _cachedLevelData = new LevelData(levelInfo);
        }

        Speed.gameObject.SetActive(displayerSettings.IsShowSpeed);
        PP.gameObject.SetActive(displayerSettings.IsShowScore);
    }

    protected override void OnHit(HitMargin hit)
    {
        RegisterJudgement(hit);

        if (judgements.Deaths > 0)
        {
            ApplyPP(-1310);
            return;
        }

        float now = Time.unscaledTime;
        if (now - lastScoreUpdateTime < ScoreUpdateInterval)
        {
            return;
        }
        lastScoreUpdateTime = now;

        if (_cachedLevelData != null)
        {
            var passData = new PassData
            {
                IsNoHoldTap = Persistence.holdBehavior == HoldBehavior.NoHoldNeeded,
                Judgements = judgements,
                Speed = CurrentLevelSpeed
            };

            double computedScore = ScoreCalculator.GetScoreV2(passData, _cachedLevelData);
            ApplyPP(computedScore);
        }
    }
    protected override void OnLoadCustomSettings(IngameElementModel model)
    {
        displayerSettings = model.GetCategory("PPDisplayer", new PPDisplayerSettingsCategory());
    }

    #endregion

    #region Visual Application

    public void ApplyPP(double Score)
    {
        if (PP == null) return;

        if (Score == -1310)
        {
            PP.text = "You died L";
            return;
        }

        if (_scoreTween != null && _scoreTween.IsActive())
        {
            _scoreTween.Kill();
        }

        _scoreTween = DOTween.To(() => (float)curScore, x =>
        {
            curScore = x;
            PP.text = curScore.ToString("0.00");
        }, (float)Score, 0.1f);
    }

    public void ApplySpped(float speed)
    {
        if (Speed != null)
        {
            Speed.text = speed.ToString("0.00") + "x";
        }
    }

    #endregion

    private void RegisterJudgement(HitMargin hit)
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

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_scoreTween != null && _scoreTween.IsActive())
        {
            _scoreTween.Kill();
        }
    }

    #region Score Engine (Nested Classes)

    public static class ScoreCalculator
    {
        private const double GmConst = 315.0;
        private const int Start = 1;
        private const int End = 50;
        private const int StartDeduc = 10;
        private const int EndDeduc = 50;
        private const double Pwr = 0.7;

        public static double GetScoreV2(PassData passData, LevelData levelData)
        {
            var inputs = passData.Judgements;
            double accuracy = CalcAcc(inputs);

            double ppBase = levelData.GetPPBaseScore();
            double standardBase = levelData.GetBaseScore();

            Main.Logger.Log("BaseScore: " + standardBase);

            double effectiveBaseScore = (accuracy >= 0.999999) ? ppBase : standardBase;

            var xaccMtp = GetXaccMtp(inputs, effectiveBaseScore);

            var speedMtp = GetSpeedMtp(passData.Speed);
            double scoreOrig = effectiveBaseScore * xaccMtp * speedMtp;

            var mtp = GetScoreV2Mtp(inputs);

            if (passData.IsNoHoldTap)
                mtp *= 0.9;

            return scoreOrig * mtp;
        }

        private static double GetScoreV2Mtp(Judgements input)
        {
            int misses = input.EarlyDouble;
            int tiles = TileCount(input);

            if (misses == 0) return 1.1;

            double tp = (Start + End) / 2.0;
            double tpDeduc = (StartDeduc + EndDeduc) / 2.0;
            double am = Math.Max(0, misses - Math.Floor(tiles / GmConst));

            if (am == 0) return 1;
            if (am <= Start) return 1 - StartDeduc / 100.0;

            if (am <= tp)
            {
                double kOne = Math.Pow((am - Start) / (tp - Start), Pwr) * (tpDeduc - StartDeduc) / 100.0;
                return 1 - StartDeduc / 100.0 - kOne;
            }
            else if (am <= End)
            {
                double kTwo = Math.Pow((End - am) / (End - tp), Pwr) * (EndDeduc - tpDeduc) / 100.0;
                return 1 + kTwo - EndDeduc / 100.0;
            }

            return 1 - EndDeduc / 100.0;
        }

        public static double GetXaccMtp(Judgements input, double effectiveBaseScore)
        {
            double xacc = CalcAcc(input);
            double xaccPercentage = xacc * 100;

            if (xaccPercentage < 95) return 1;
            if (xaccPercentage < 100) return -0.027 / (xacc - 1.0054) + 0.513;

            if (xaccPercentage >= 99.9999)
            {
                double a = 2100;
                double k = 14;
                double h = -a / (k - 6);

                if (Math.Abs(effectiveBaseScore - h) < 0.0001) return k;

                return (-a) / (effectiveBaseScore - h) + k;
            }

            return 1;
        }

        private static double GetSpeedMtp(double speed)
        {
            if (speed == 1 || speed == 0) return 1;
            if (speed < 1) return 0;

            if (speed < 1.1)
                return -3.5 * speed + 4.5;
            if (speed < 1.5)
                return 0.65;
            if (speed < 2)
                return 0.7 * speed - 0.4;

            return 1;
        }

        private static int TileCount(Judgements data)
        {
            return FloorCount;
        }

        public static double CalcAcc(Judgements input)
        {
            int totalHits = input.EarlyDouble + input.LateDouble +
                            input.EarlySingle + input.LateSingle +
                            input.EPerfect + input.LPerfect +
                            input.Perfect;

            if (totalHits == 0) return 0;

            return (
                (1.0 * input.Perfect) +
                (0.75 * input.EPerfect) +
                (0.75 * input.LPerfect) +
                (0.4 * input.EarlySingle) +
                (0.4 * input.LateSingle) +
                (0.2 * input.EarlyDouble) +
                (0.2 * input.LateDouble)
            ) / totalHits;
        }

        public static double CalcAcc(PassesListInfoElementJudgementsJson input)
        {
            return CalcAcc(new Judgements
            {
                Perfect = input.Perfect,
                EPerfect = input.EPerfect,
                EarlySingle = input.EarlySingle,
                LateSingle = input.LateSingle,
                EarlyDouble = input.EarlyDouble,
                LateDouble = input.LateDouble,
                LPerfect = input.LPerfect,
                Deaths = input.Deaths
            });
        }
    }

    public class Judgements
    {
        public int EarlyDouble { get; set; }
        public int EarlySingle { get; set; }
        public int EPerfect { get; set; }
        public int Perfect { get; set; }
        public int LPerfect { get; set; }
        public int LateSingle { get; set; }
        public int LateDouble { get; set; }
        public int Deaths { get; set; }

        public void Reset()
        {
            EarlyDouble = EarlySingle = LateDouble = LateSingle = Perfect = EPerfect = LPerfect = Deaths = 0;
        }
    }

    public class PassData
    {
        public double Speed { get; set; } = 1.0;
        public Judgements Judgements { get; set; } = new();
        public bool IsNoHoldTap { get; set; } = false;
    }

    public class LevelData
    {
        public double BaseScore { get; set; }
        public double PPBaseScore { get; set; }

        public double GetBaseScore() => BaseScore;
        public double GetPPBaseScore() => PPBaseScore;

        public LevelData(LevelListInfoElementJson levelInfo)
        {
            if (levelInfo.BaseScore != null && levelInfo.BaseScore > 0)
                BaseScore = levelInfo.BaseScore.Value;
            else if (levelInfo.Difficulty != null && levelInfo.Difficulty.BaseScore > 0)
                BaseScore = levelInfo.Difficulty.BaseScore;
            else
                BaseScore = 0;

            if (levelInfo.PPBaseScore != null && levelInfo.PPBaseScore > 0)
                PPBaseScore = levelInfo.PPBaseScore.Value;
            else if (levelInfo.BaseScore != null && levelInfo.BaseScore > 0)
                PPBaseScore = levelInfo.BaseScore.Value;
            else if (levelInfo.Difficulty != null && levelInfo.Difficulty.BaseScore > 0)
                PPBaseScore = levelInfo.Difficulty.BaseScore;
            else
                PPBaseScore = 0;
        }
    }

    #endregion
}

public class PPDisplayerSettingsCategory : IngameElementSettingsCategory
{
    public override string DisplayName => "Speed & Score";

    private bool _isShowSpeed = true;
    public bool IsShowSpeed
    {
        get => _isShowSpeed;
        set
        {
            _isShowSpeed = value;
            OnPropertyChanged();
        }
    }

    private bool _isShowScore = true;
    public bool IsShowScore
    {
        get => _isShowScore;
        set
        {
            _isShowScore = value;
            OnPropertyChanged();
        }
    }
}