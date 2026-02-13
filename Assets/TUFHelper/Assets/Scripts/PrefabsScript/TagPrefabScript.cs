using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TagPrefabScript : MonoBehaviour
{
    public Image iconTag;
    public TextMeshProUGUI tagText;
    public string tagTUF;

    public Image backgroundImage;

    private Color selectedColor = new(1, 1, 1, 40 / 255f);
    private Color unselectedColor = new(1, 1, 1, 20 / 255f);

    private bool isSelected = false;
    public bool IsSelected
    {
        get { return isSelected; } 
        set
        {
            if (value) backgroundImage.color = selectedColor;
            else backgroundImage.color = unselectedColor;

            isSelected = value; 
        }
    }

    public void SetTagInfo(bool isSelected, string tag, Sprite icon)
    {
        IsSelected = isSelected;
        tagText.text = tag;
        iconTag.sprite = icon;
        tagTUF = tag;
    }
}
