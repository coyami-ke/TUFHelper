using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using TUFHelper.ModScripts.Json;
using UnityEngine;
using UnityEngine.UI;

public class IngamerankPrefabScript : MonoBehaviour
{
    public TextMeshProUGUI accuracyText, scoreText, nicknameText, rankText;
    public RectTransform rectTransform;
    public Image gradeImage;
    public Sprite PP, SS, S, A, B, C, D;

    public PassesListInfoElementJson PassInfo { get; private set; }

    private int rank;
    public int Rank
    {
        get => rank;
        set
        {
            if (value == rank) return;
            rank = value;
            rankText.text = $"#{rank}";
        }
    }

    public void LoadPass(PassesListInfoElementJson info, int rank)
    {
        PassInfo = info;
        Rank = rank;
        UpdateVisual();
    }

    private float lastAcc = -1f;
    private float lastScore = -1f;

    public void UpdateVisual()
    {
        if (PassInfo == null) return;

        if (Mathf.Abs(PassInfo.Accuracy - lastAcc) > 0.00001f)
        {
            lastAcc = PassInfo.Accuracy;
            accuracyText.text = $"{lastAcc * 100f:F2}%";
            gradeImage.sprite = GetGradeSprite(lastAcc);
        }

        if (Mathf.Abs(PassInfo.ScoreV2 - lastScore) > 0.1f)
        {
            lastScore = PassInfo.ScoreV2;
            scoreText.text = lastScore.ToString("F2");
        }

        if (string.IsNullOrEmpty(nicknameText.text) || nicknameText.text == "nickname")
        {
            nicknameText.text = PassInfo.Player?.Name ?? "Unknown";
            nicknameText.color = (nicknameText.text == "YOU") ? Color.yellow : Color.white;
        }
    }



    private Sprite GetGradeSprite(float acc)
    {
        if (acc == 1f) return PP;
        if (acc > 0.998f) return SS;
        if (acc > 0.995f) return S;
        if (acc > 0.990f) return A;
        if (acc > 0.980f) return B;
        if (acc > 0.975f) return C;
        return D;
    }

    private float lastTargetY = float.MinValue;
    public void SetPosition(int visualIndex)
    {
        float targetY = visualIndex * -60f - 10f;

        if (Mathf.Abs(targetY - lastTargetY) < 0.1f) return;

        lastTargetY = targetY;

        rectTransform.DOKill();
        rectTransform.DOAnchorPosY(targetY, 0.5f).SetEase(Ease.OutExpo);
    }
}
