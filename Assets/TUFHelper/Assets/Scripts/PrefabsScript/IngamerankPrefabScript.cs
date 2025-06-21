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
            rankText.text = "#" + rank.ToString();

            rectTransform.localScale = Vector3.one;
            rectTransform.sizeDelta = new Vector2(0, 60);

            rectTransform.DOAnchorPosY((rank - 1) * -60 - 10, 0.5f).SetEase(Ease.OutBack);
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
        accuracyText.text = (PassInfo.Accuracy * 100).ToString("F2") + "%";
        scoreText.text = PassInfo.ScoreV2.ToString("F2");
        nicknameText.text = PassInfo.Player.Name;

        if (PassInfo.Accuracy == 1.0f)
        {
            gradeImage.sprite = X;
        }
        else if (PassInfo.Accuracy > 0.998f)
        {
            gradeImage.sprite = SS;
        }
        else if (PassInfo.Accuracy > 0.995f)
        {
            gradeImage.sprite = S;
        }
        else if (PassInfo.Accuracy > 0.990f)
        {
            gradeImage.sprite = A;
        }
        else if (PassInfo.Accuracy > 0.980f)
        {
            gradeImage.sprite = B;
        }
        else if (PassInfo.Accuracy > 0.975f)
        {
            gradeImage.sprite = C;
        }
        else
        {
            gradeImage.sprite = D;
        }
    }
}
