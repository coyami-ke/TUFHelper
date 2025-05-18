using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DownloadPanelAnimIcon : MonoBehaviour
{
    public RectTransform rectTransform;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform.DORotate(new Vector3(0, 0 , 360), 1.5f, RotateMode.FastBeyond360).SetEase(Ease.InOutBack).SetLoops(-1, LoopType.Yoyo);
        rectTransform.DOAnchorPosX(-200, 1.5f).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
