using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelPrefabClickScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public LevelPrefabScript lps;
    public Image image;

    public void Awake()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        lps.InfoButtonClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.DOColor(new Color(1f, 1f, 1f, 80 / 255f), 0.5f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.DOColor(new Color(1f, 1f, 1f, 40 / 255f), 0.5f).SetEase(Ease.OutExpo);
    }

    public void Update()
    {
        
    }
}
