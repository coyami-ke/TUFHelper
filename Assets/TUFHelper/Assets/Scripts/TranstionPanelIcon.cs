using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TranstionPanelIcon : MonoBehaviour
{
    private RectTransform rectTransform;

    private float originalPosY;

    public float speed = 15f; // degrees per second (can be negative for reverse)

    public void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        originalPosY = rectTransform.anchoredPosition.y;

        // Bobbing animation (forever)
        rectTransform
            .DOAnchorPosY(originalPosY + Random.Range(-55f, 55f), 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
