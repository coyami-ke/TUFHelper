using System;
using DG.Tweening;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IngameElementInListScript : MonoBehaviour, IPointerClickHandler
{
    public class IngameElementInListSelectedEventArgs : EventArgs
    {

    }
    public event EventHandler<IngameElementInListSelectedEventArgs> Selected;

    public TextMeshProUGUI text;
    public Image showImage, icon, backgroundImage, lineImage;
    public Sprite showOnSprite, showOffSprite;

    public BasicIngameElement Element {  get; private set; }

    private bool _isSelected = false;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;

            _isSelected = value;
            if (value)
            {
                lineImage.rectTransform.DOScaleX(1f, 0.5f).SetEase(Ease.OutExpo);
                Selected?.Invoke(this, new());
            }
            else
                lineImage.rectTransform.DOScaleX(0f, 0.5f).SetEase(Ease.OutExpo);
        }
    }

    public void SetElementInfo(BasicIngameElement element)
    {
        text.text = element.NameInSettings;
        icon.sprite = element.Icon;
        Element = element;
    }

    public void Start()
    {
        UpdateEyeVisual();
    }

    public void OnEyePressed()
    {
        if (Element == null || Element.Model == null) return;

        Element.Model.IsShowed = !Element.Model.IsShowed;
        UpdateEyeVisual();
    }

    public void UpdateEyeVisual()
    {
        if (Element == null || Element.Model == null) return;

        if (!Element.Model.IsShowed)
        {
            showImage.sprite = showOffSprite;
        }
        else
        {
            showImage.sprite = showOnSprite;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsSelected) return;

        if (transform.parent != null)
        {
            foreach (Transform sibling in transform.parent)
            {
                var siblingScript = sibling.GetComponent<IngameElementInListScript>();
                if (siblingScript != null && siblingScript != this)
                {
                    siblingScript.IsSelected = false;
                }
            }
        }

        IsSelected = true;
    }
}