using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RankPrefabScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum GradesScore
    {
        PP,
        SS,
        S,
        A,
        B,
        C,
        D,
    }

    private static readonly Dictionary<GradesScore, Color> GradeColors = new()
    {
        { GradesScore.PP, new Color(0.741f, 0.247f, 1.000f) }, // #BD3FFF
        { GradesScore.SS, new Color(1.000f, 0.357f, 0.565f) }, // #FF5B90
        { GradesScore.S,  new Color(1.000f, 0.761f, 0.161f) }, // #FFC229
        { GradesScore.A,  new Color(1.000f, 0.169f, 0.459f) }, // #FF2B75
        { GradesScore.B,  new Color(0.184f, 0.529f, 1.000f) }, // #2F87FF
        { GradesScore.C,  new Color(0.114f, 0.941f, 0.463f) }, // #1DF076
        { GradesScore.D,  new Color(1.000f, 0.533f, 0.200f) }  // #FF8833
    };

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;

            background.DOKill();
            float targetAlpha = value ? 20f / 255f : 10f / 255f;
            background.DOColor(new Color(1f, 1f, 1f, targetAlpha), 0.5f).SetEase(Ease.OutExpo);

            if (value)
            {
                if (WindowsManager.instance != null) WindowsManager.instance.MoveToPassInfo();
                if (global::PassInfo.instance != null) global::PassInfo.instance.SetPassInfo(PassInfo, LevelInfo);
            }
        }
    }

    public PassesListInfoElementJson PassInfo { get; private set; }
    public LevelListInfoElementJson LevelInfo { get; private set; }

    [Header("Text Fields")]
    //public TextMeshProUGUI perfectText;
    //public TextMeshProUGUI lPerfectText;
    //public TextMeshProUGUI ePerfectText;
    //public TextMeshProUGUI lateText;
    //public TextMeshProUGUI earlyText;
    //public TextMeshProUGUI tooEarlyText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI playerText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI rankText;

    [Header("UI Images")]
    public Image flag;
    public Image grade;
    public Image background;
    public Image leftRectangle;

    public void SetPassInfo(PassesListInfoElementJson pass, LevelListInfoElementJson level, int rank)
    {
        if (pass == null) return;

        PassInfo = pass;
        LevelInfo = level;

        //if (perfectText != null) perfectText.text = pass.Judgements.Perfect.ToString();
        //if (ePerfectText != null) ePerfectText.text = pass.Judgements.EPerfect.ToString();
        //if (lPerfectText != null) lPerfectText.text = pass.Judgements.LPerfect.ToString();
        //if (lateText != null) lateText.text = pass.Judgements.LateSingle.ToString();
        //if (earlyText != null) earlyText.text = pass.Judgements.EarlySingle.ToString();
        //if (tooEarlyText != null) tooEarlyText.text = pass.Judgements.EarlyDouble.ToString();

        if (speedText != null) speedText.text = $"{pass.Speed:F2}x";
        if (dateText != null) dateText.text = RelativeTimeFormatter.ToRelativeTime(pass.CreatedAt);
        if (scoreText != null) scoreText.text = $"{pass.ScoreV2:F1}";

        if (playerText != null)
        {
            playerText.text = pass.Player?.Name ?? string.Empty;
            LanguageManager.ApplyChineseJapaneseFont(playerText);
        }

        if (accuracyText != null) accuracyText.text = $"{pass.Accuracy * 100f:F2}%";
        if (rankText != null) rankText.text = $"#{rank}";

        GradesScore calculatedGrade = GetGradeFromAccuracy(pass.Accuracy);

        if (grade != null)
        {
            grade.sprite = GetGradeSprite(calculatedGrade);
        }

        if (flag != null && pass.Player != null)
        {
            flag.sprite = Helper.GetFlagSprite(pass.Player.Country);
        }

        if (leftRectangle != null)
        {
            Color baseColor = GradeColors.TryGetValue(calculatedGrade, out Color gradeColor)
                ? gradeColor
                : Color.white;

            leftRectangle.color = new Color(baseColor.r, baseColor.g, baseColor.b, leftRectangle.color.a);
        }
    }

    private static GradesScore GetGradeFromAccuracy(float accuracy)
    {
        if (accuracy >= 1.0f) return GradesScore.PP;
        if (accuracy > 0.998f) return GradesScore.SS;
        if (accuracy > 0.995f) return GradesScore.S;
        if (accuracy > 0.990f) return GradesScore.A;
        if (accuracy > 0.980f) return GradesScore.B;
        if (accuracy > 0.975f) return GradesScore.C;
        return GradesScore.D;
    }

    public static Sprite GetGradeSprite(GradesScore grade)
    {
        string path = $"Assets/TUFHelper/Assets/Sprites/Grades/Grade_{grade}.png";
        return Main.GetSpriteFromAssets(path);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InfoButtonClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsSelected && background != null)
        {
            background.DOKill();
            background.DOColor(new Color(1f, 1f, 1f, 20f / 255f), 0.5f).SetEase(Ease.OutExpo);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsSelected && background != null)
        {
            background.DOKill();
            background.DOColor(new Color(1f, 1f, 1f, 10f / 255f), 0.5f).SetEase(Ease.OutExpo);
        }
    }

    public void InfoButtonClick()
    {
        if (!IsSelected)
        {
            if (LeaderboardScript.instance?.passListParent != null)
            {
                var siblingPasses = LeaderboardScript.instance.passListParent.GetComponentsInChildren<RankPrefabScript>();
                foreach (var pass in siblingPasses)
                {
                    if (pass != this) pass.IsSelected = false;
                }
            }

            IsSelected = true;
        }
    }

    private void OnDestroy()
    {
        if (background != null) background.DOKill();
    }
}