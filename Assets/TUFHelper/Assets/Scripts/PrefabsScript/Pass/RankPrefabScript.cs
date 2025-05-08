using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class RankPrefabScript : MonoBehaviour
{
    public enum GradesScore
    {
        X,
        SS,
        S,
        A,
        B,
        C,
        D,
    }
    public PassesListInfoElementJson PassInfo { get; private set; }

    public TextMeshProUGUI perfectText, lPerfectText, ePerfectText, lateText, earlyText, tooEarlyText, speedText, dateText, scoreText, playerText, accuracyText, rankText;
    public Image flag, grade;
    public void SetPassInfo(PassesListInfoElementJson pass, int rank)
    {
        perfectText.text = pass.Judgements.Perfect.ToString();
        ePerfectText.text = pass.Judgements.EPerfect.ToString();
        lPerfectText.text = pass.Judgements.LPerfect.ToString();
        lateText.text = pass.Judgements.LateSingle.ToString();
        earlyText.text = pass.Judgements.EarlySingle.ToString();
        tooEarlyText.text = pass.Judgements.EarlyDouble.ToString();
        speedText.text = pass.Speed.ToString("F2") + "x";
        dateText.text = pass.CreatedAt;
        scoreText.text = pass.ScoreV2.ToString("F1");
        playerText.text = pass.Player.Name;
        accuracyText.text = (pass.Accuracy * 100).ToString("F2") + "%"; 
        rankText.text = $"#{rank}";

        PassInfo = pass;

        if (pass.Accuracy == 1.0f)
        {
            grade.sprite = GetGradeSprite(GradesScore.X);
        }
        else if (pass.Accuracy > 0.998f)
        {
            grade.sprite = GetGradeSprite(GradesScore.SS);
        }
        else if (pass.Accuracy > 0.995f)
        {
            grade.sprite = GetGradeSprite(GradesScore.S);
        }
        else if (pass.Accuracy > 0.990f)
        {
            grade.sprite = GetGradeSprite(GradesScore.A);
        }
        else if (pass.Accuracy > 0.980f)
        {
            grade.sprite = GetGradeSprite(GradesScore.B);
        }
        else if (pass.Accuracy > 0.975f)
        {
            grade.sprite = GetGradeSprite(GradesScore.C);
        }
        else
        {
            grade.sprite = GetGradeSprite(GradesScore.D);
        }
        flag.sprite = Helper.GetFlagSprite(pass.Player.Country);
    }
    private Sprite GetGradeSprite(GradesScore grade)
    {
        string path = "Assets/TUFHelper/Assets/Sprites/Grades/Grade_" + grade + ".png";
        return Main.assets.LoadAsset<Sprite>(path);
    }
}
