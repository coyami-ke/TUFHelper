using System;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using TUFHelper.ModScripts.Json;
using UnityEngine;
using TUFHelper;

public class PPDisplayerScript : MonoBehaviour
{
    public TextMeshProUGUI PP, Speed;
    public LevelListInfoElementJson Levelinfo;

    private double curScore;
    private Tween _scoreTween;

    public static List<float> currentAnglePath = new();
    public static string currentPathdata;
    public static int FloorCount;

    static ADOFAI.LevelData leveldata
    {
        get => scnGame.instance.levelData;
    }

    private void Awake()
    {
        PP.text = string.Empty;
        Speed.text = string.Empty;

        if (leveldata != null)
        {
            currentAnglePath = leveldata.angleData;
            currentPathdata = leveldata.pathData;
        }
    }

    public void ApplyPP(double Score)
    {
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
        Speed.text = speed.ToString("0.00") + "x";
    }

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
}