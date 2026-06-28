using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameElementInListScript : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image showImage, icon, backgroundImage;
    public Sprite showOnSprite, showOffSprite;

    private BasicIngameElement _element;

    public void SetElementInfo(BasicIngameElement element)
    {
        text.text = element.NameInSettings;
        icon.sprite = element.Icon;

        //if (element.Model.IsShowed)
        //    showImage.sprite = showOnSprite;
        //else
        //    showImage.sprite = showOffSprite;

        _element = element;

    }

    public void Start()
    {
        UpdateEyeVisual();
    }

    public void OnEyePressed()
    {
        _element.Model.IsShowed = !_element.Model.IsShowed;

        UpdateEyeVisual();
    }

    public void UpdateEyeVisual()
    {
        if (!_element.Model.IsShowed)
        {
            showImage.sprite = showOffSprite;
        }
        else
        {
            showImage.sprite = showOnSprite;
        }
    }
}
