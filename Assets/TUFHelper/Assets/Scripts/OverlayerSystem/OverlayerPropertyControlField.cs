using System;
using TMPro;
using UnityEngine;

public abstract class OverlayerPropertyControlField : MonoBehaviour
{
    public TextMeshProUGUI propertyLabelText;

    public abstract void BindProperty(string labelName, object currentValue, Action<object> onValueChanged);
}
