using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper.ModScripts.Json;
using UnityEngine;
using UnityEngine.UI;
using static IngameLeaderboardScript;

public class IngamerankPrefabScript : MonoBehaviour
{
    public TextMeshProUGUI accuracyText, scoreText, nicknameText, rankText;
    public RectTransform rectTransform;
    public Image gradeImage;
    public Sprite PP, SS, S, A, B, C, D;

    public IngameLeaderboardRank PassInfo { get; set; }

    public void LoadPass(IngameLeaderboardRank info, int rank)
    {
        PassInfo = info;
        rankText.text = $"#{rank + 1}";
        UpdateVisual();
    }

    private float lastAcc = float.NaN;
    private float lastScore = float.NaN;
    private string lastNickname = null;

    public void UpdateVisual()
    {
        if (!Mathf.Approximately(PassInfo.accuracy, lastAcc))
        {
            lastAcc = PassInfo.accuracy;
            accuracyText.text = $"{lastAcc * 100f:F2}%";
            gradeImage.sprite = GetGradeSprite(lastAcc);
        }

        if (!Mathf.Approximately(PassInfo.scoreV2, lastScore))
        {
            lastScore = PassInfo.scoreV2;
            scoreText.text = lastScore.ToString("F2");
        }

        if (lastNickname == null)
        {
            lastNickname = PassInfo.player.name.IsEmpty ? "Unknown" : new string(PassInfo.player.name.Span);
            nicknameText.text = lastNickname;
            nicknameText.color = lastNickname == "YOU" ? Color.yellow : Color.white;
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

    public void SetRank(int rank)
    {
        rankText.text = '#' + rank.ToString();
    }
}
