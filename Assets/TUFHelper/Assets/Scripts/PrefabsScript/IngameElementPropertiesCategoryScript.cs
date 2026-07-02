using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameElementPropertiesCategoryScript : MonoBehaviour
{
    public Image backgroundImage, lineImage, iconImage;
    public TextMeshProUGUI text;

    private bool _isSelected = false;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            if (value)
            {
                lineImage.rectTransform.DOSizeDelta(new(220, 2), 0.5f).SetEase(Ease.OutExpo);
            }
            else
            {
                lineImage.rectTransform.DOSizeDelta(new(0, 2), 0.5f).SetEase(Ease.OutExpo);
            }
        }
    }

    public void SetCategory(IngameElementSettingsCategory category)
    {
        text.text = category.DisplayName;
        iconImage.sprite = category.Icon;
    }
}
