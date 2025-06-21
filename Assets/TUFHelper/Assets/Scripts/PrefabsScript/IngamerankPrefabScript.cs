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
    public Sprite X, SS, S, A, B, C, D;

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

    public void UpdateVisual()
    {
        accuracyText.text = (PassInfo.Accuracy * 100f).ToString("F2") + "%";
        scoreText.text = PassInfo.ScoreV2.ToString("F2");
        nicknameText.text = PassInfo.Player.Name;

        if (PassInfo.Player.Name == "YOU") nicknameText.color = Color.yellow;

        gradeImage.sprite = GetGradeSprite(PassInfo.Accuracy);
    }

    private Sprite GetGradeSprite(float acc)
    {
        if (acc == 1f) return X;
        if (acc > 0.998f) return SS;
        if (acc > 0.995f) return S;
        if (acc > 0.990f) return A;
        if (acc > 0.980f) return B;
        if (acc > 0.975f) return C;
        return D;
    }

    // Called from manager to update position in UI list
    public void SetPosition(int visualIndex)
    {
        float y = visualIndex * -60f - 10f;
        rectTransform.DOAnchorPosY(y, 1f).SetEase(Ease.OutExpo);
    }
}
