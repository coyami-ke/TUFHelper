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
    public static double curScore;

    public static List<float> currentAnglePath = new();
    public static string currentPathdata;
    public static int FloorCount;

    static ADOFAI.LevelData leveldata
    {
        get
        {
            return scnGame.instance.levelData;
        }
    }

    // Assign some values
    private void Awake()
    {
        PP.text = string.Empty;
        Speed.text = string.Empty;

        currentAnglePath = leveldata.angleData;
        currentPathdata = leveldata.pathData;
    }


    private void Update()
    {

    }
    public void ApplyPP(double Score)
    {
        bool flag = Score == -1310;

        if (!flag)
        {
            // Animate the score
            // Kill any existing tween to avoid overlaps
            DOTween.Kill(PP);

            // Tween from current value to target value
            DOTween.To(() => curScore, x =>
            {
                curScore = x;
                PP.text = curScore.ToString("0.00");
            }, Score, 0.1f);
        }
        else
        {
            PP.text = "You died L";
        }
    }
    // Dont ask why these function exists
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

            double effectiveBaseScore = (accuracy >= 0.999999) ? ppBase : standardBase;

            var xaccMtp = GetXaccMtp(inputs, effectiveBaseScore);

            bool isMarathon = levelData.Difficulty?.Name == "Marathon";
            var speedMtp = GetSpeedMtp(passData.Speed, isMarathon);

            double scoreOrig = isMarathon
                ? Math.Max(effectiveBaseScore * xaccMtp * speedMtp, 0)
                : effectiveBaseScore * xaccMtp * speedMtp;

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


        private static double GetSpeedMtp(double speed, bool isDesBus = false)
        {
            if (isDesBus)
            {
                if (speed == 1 || speed == 0) return 1;
                if (speed > 1) return Math.Max(2 - speed, 0);
            }

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

        // This Func is mind c:
        public static double CalcAcc(Judgements input)
        {
            // Total hits recorded so far (all judgements)
            int totalHits = input.EarlyDouble + input.LateDouble +
                            input.EarlySingle + input.LateSingle +
                            input.EPerfect + input.LPerfect +
                            input.Perfect;

            // Avoid division by zero: if no hits, assume 100%
            if (totalHits == 0)
                return 0;

            // Calculate weighted accuracy over hits so far
            double accuracy = (
                (1.0 * input.Perfect) +
                (0.75 * input.EPerfect) +
                (0.75 * input.LPerfect) +
                (0.4 * input.EarlySingle) +
                (0.4 * input.LateSingle) +
                (0.2 * input.EarlyDouble) +
                (0.2 * input.LateDouble)
            ) / totalHits;

            return accuracy;
        }
        public static double CalcAcc(PassesListInfoElementJudgementsJson input)
        {
            return CalcAcc(new Judgements() { Perfect = input.Perfect, EPerfect = input.EPerfect, EarlySingle = input.EarlySingle, LateSingle = input.LateSingle, EarlyDouble = input.EarlyDouble, LateDouble = input.LateDouble, LPerfect = input.LPerfect, Deaths = input.Deaths});
        }

    }

    // Class Helpers
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
            EarlyDouble = 0;
            EarlySingle = 0;
            LateDouble = 0;
            LateSingle = 0;
            Perfect = 0;
            EPerfect = 0;
            LPerfect = 0;
            Deaths = 0;
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
        public Difficulty Difficulty { get; set; } = new();
        public double? BaseScore { get; set; }
        public double? PPBaseScore { get; set; }

        public double GetBaseScore()
        {
            if (BaseScore == null || BaseScore == 0)
                return Difficulty.BaseScore;

            return BaseScore.Value;
        }

        public double GetPPBaseScore()
        {
            if (PPBaseScore == null || PPBaseScore == 0)
                return Difficulty.BaseScore; 

            return PPBaseScore.Value;
        }
    }

    public class Difficulty
    {
        public string Name { get; set; }
        public double BaseScore { get; set; }
    }
}
