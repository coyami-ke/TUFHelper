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
                lineImage.rectTransform.DOScaleX(1f, 0.5f).SetEase(Ease.OutExpo);
                backgroundImage.DOColor(new Color(1, 1, 1, 20 / 255f), 0.5f).SetEase(Ease.OutExpo);
            }
            else
            {
                lineImage.rectTransform.DOScaleX(0f, 0.5f).SetEase(Ease.OutExpo);
                backgroundImage.DOColor(new Color(1, 1, 1, 10 / 255f), 0.5f).SetEase(Ease.OutExpo);
            }
        }
    }

    public void SetCategory(IngameElementSettingsCategory category)
    {
        text.text = category.DisplayName;
        iconImage.sprite = category.Icon;
    }

    public void OnClicked()
    {
        IsSelected = true;
    }
}
