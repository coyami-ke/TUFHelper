using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingTabPrefabScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public string nameTab;
    public Image backgroundImage;
    public RectTransform rectTransformLine;

    public GameObject settingsObject;

    private bool _isSelected = false;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;

            // Only deselect other tabs, not this one
            foreach (var tab in ModSettings.instance.tabs)
            {
                if (tab != this)
                {
                    tab.IsSelected = false;
                    tab.settingsObject.SetActive(false);
                }
            }

            _isSelected = value;

            rectTransformLine
                .DOScaleX(value ? 1 : 0, 0.5f)
                .SetEase(Ease.OutExpo);

            if (value) // only update CurrentTab when selecting
                ModSettings.instance.CurrentTab = this;
                
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        IsSelected = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsSelected) backgroundImage.DOColor(new Color(1f, 1f, 1f, 20 / 255f), 0.5f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsSelected) backgroundImage.DOColor(new Color(1f, 1f, 1f, 10 / 255f), 0.5f).SetEase(Ease.OutExpo);
    }
}
