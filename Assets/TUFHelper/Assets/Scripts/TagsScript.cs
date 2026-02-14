using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

public class TagsScript : MonoBehaviour, IPointerClickHandler
{
    public RectTransform menuTransform;
    public RectTransform arrowTransfrom;

    public Image backgroundImage;

    private const float selectedRotation = -90;
    private const float unselectedRotation = 90;

    private readonly Color selectedColor = new(1f, 1f, 1f, 20f / 255f);
    private readonly Color unselectedColor = new(1f, 1f, 1f, 10f / 255f);

    private bool isSelected = false;
    public bool IsSelected
    {
        get { return isSelected; }
        set
        {
            isSelected = value;
            AnimateDropdown(value);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        IsSelected = !IsSelected;
    }

    private void AnimateDropdown(bool show)
    {
        backgroundImage.DOColor(show ? selectedColor : unselectedColor, 0.4f).SetEase(Ease.OutExpo);
        arrowTransfrom.DORotate(new Vector3(0f, 0f, show ? selectedRotation : unselectedRotation), 0.4f).SetEase(Ease.OutExpo);
        menuTransform.DOScaleY(show ? 1f : 0f, 0.4f).SetEase(Ease.OutExpo);
    }
}
