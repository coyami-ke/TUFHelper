using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

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
            UITransition.AnimateRankSelection(transform, value);

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

    private RectTransform _rectTransform;
    private ScrollRect _scrollRect;
    private RectTransform _viewportTransform;
    private Transform _contentTransform;
    private GameObject _scrollView;

    private readonly Vector3[] _itemCorners = new Vector3[4];
    private readonly Vector3[] _viewportCorners = new Vector3[4];
    public GameObject visualContainer;

    public void Init(GameObject scrollViewObj, Transform contentTransform)
    {
        _rectTransform = GetComponent<RectTransform>();
        _scrollView = scrollViewObj;
        _contentTransform = contentTransform;

        _scrollRect = _scrollView.GetComponent<ScrollRect>();
        _viewportTransform = _scrollRect.viewport;
        _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

        OnScrollValueChanged(new());
    }

    private void OnScrollValueChanged(Vector2 pos)
    {
        bool isVisible = IsVisibleInScrollView();

        visualContainer.SetActive(isVisible);
    }

    public bool IsVisibleInScrollView()
    {
        _rectTransform.GetWorldCorners(_itemCorners);
        _viewportTransform.GetWorldCorners(_viewportCorners);

        return _itemCorners[1].y >= _viewportCorners[0].y && _itemCorners[0].y <= _viewportCorners[1].y;
    }

    public void SetPassInfo(PassesListInfoElementJson pass, LevelListInfoElementJson level, int rank)
    {
        if (pass == null) return;

        PassInfo = pass;
        LevelInfo = level;

        speedText.text = $"{pass.speed:F2}x";
        dateText.text = RelativeTimeFormatter.ToRelativeTime(pass.createdAt);
        scoreText.text = $"{pass.scoreV2:F1}";
        playerText.text = pass.player?.name ?? string.Empty;

        accuracyText.text = $"{pass.accuracy * 100f:F2}%";
        rankText.text = $"#{rank}";

        GradesScore calculatedGrade = GetGradeFromAccuracy(pass.accuracy);

        grade.sprite = GetGradeSprite(calculatedGrade);

        flag.sprite = Helper.GetFlagSprite(pass.player.country);

        Color baseColor = GradeColors.TryGetValue(calculatedGrade, out Color gradeColor)
                ? gradeColor
                : Color.white;

        leftRectangle.color = new Color(baseColor.r, baseColor.g, baseColor.b, leftRectangle.color.a);

        UITransition.AnimateLeaderboardItem(this);
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
