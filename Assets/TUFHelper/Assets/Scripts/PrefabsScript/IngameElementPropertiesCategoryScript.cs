using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameElementPropertiesCategoryScript : MonoBehaviour
{
    public Image backgroundImage, lineImage, iconImage;
    public TextMeshProUGUI text;

    private Transform categoryPropertiesTransform;

    public IngameElementSettingsCategory Category { get; private set; }
    public object TargetObject { get; private set; }

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
                backgroundImage.DOColor(new Color(1f, 1f, 1f, 20f / 255f), 0.5f).SetEase(Ease.OutExpo);
            }
            else
            {
                lineImage.rectTransform.DOScaleX(0f, 0.5f).SetEase(Ease.OutExpo);
                backgroundImage.DOColor(new Color(1f, 1f, 1f, 10f / 255f), 0.5f).SetEase(Ease.OutExpo);
            }
        }
    }

    public void SetModelTarget(string displayName, object modelTarget, Transform propertiesParent)
    {
        categoryPropertiesTransform = propertiesParent;
        text.text = displayName;
        //if (iconImage != null) iconImage.gameObject.SetActive(false); 

        TargetObject = modelTarget;
    }
    
    public void SetCategory(IngameElementSettingsCategory category, Transform propertiesParent)
    {
        categoryPropertiesTransform = propertiesParent;
        text.text = category.DisplayName;
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(category.Icon != null);
            iconImage.sprite = category.Icon;
        }

        TargetObject = category;
    }

    public void OnClicked()
    {
        if (IsSelected) return;

        if (transform.parent != null)
        {
            foreach (Transform sibling in transform.parent)
            {
                var siblingScript = sibling.GetComponent<IngameElementPropertiesCategoryScript>();
                if (siblingScript != null && siblingScript != this)
                {
                    siblingScript.IsSelected = false;
                }
            }
        }

        IsSelected = true;

        if (categoryPropertiesTransform != null)
        {
            var inspectorEngine = categoryPropertiesTransform.GetComponent<OverlayerCategoryPropertiesInspector>();
            if (inspectorEngine != null)
            {
                inspectorEngine.GenerateInspectorUI(TargetObject);
            }
        }
    }
}