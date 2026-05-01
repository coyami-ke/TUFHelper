using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TransitionPanelFrontPage : MonoBehaviour
{
    public GameObject transitionPanel;

    private RectTransform rectTransform;

    public static TransitionPanelFrontPage instance { get; set; }

    private bool _isShow = false;
    public bool IsShow
    {
        get => _isShow;
        set
        {
            if (value == _isShow) return;
            _isShow = value;

            if (_isShow)
                ShowPanel();
            else
                HidePanel();
        }
    }

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        rectTransform = transitionPanel.GetComponent<RectTransform>();

        // start hidden above screen
        rectTransform.anchoredPosition = new Vector2(0, rectTransform.rect.height);
    }

    private void ShowPanel()
    {
        rectTransform.DOAnchorPos(Vector2.zero, 0.5f)
            .SetEase(Ease.OutCubic);
    }

    private void HidePanel()
    {
        float height = rectTransform.rect.height;
        rectTransform.DOAnchorPos(new Vector2(0, height), 0.5f)
            .SetEase(Ease.InCubic);
    }
}
