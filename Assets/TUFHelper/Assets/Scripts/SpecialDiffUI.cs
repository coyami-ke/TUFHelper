using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SpecialDiffUI : MonoBehaviour
{
    public Image backgroundImage;

    private Color selectedColor = new(1, 1 ,1, 40 / 255f);
    private Color unselectedColor = new(1, 1, 1, 20 / 255f);

    public void Start()
    {
        //OnValueChanged(false);
    }
    public void OnValueChanged(bool value)
    {
        if (value)
        {
            backgroundImage.DOColor(selectedColor, 0.4f).SetEase(Ease.OutExpo);
        }
        else
        {
            backgroundImage.DOColor(unselectedColor, 0.4f).SetEase(Ease.OutExpo);
        }
    }
}
