using System;
using UnityEngine;
using UnityEngine.UI;

public class OverlayerTogglePropertyControl : OverlayerPropertyControlField
{
    public Toggle toggleInput;

    public override void BindProperty(string labelName, object currentValue, Action<object> onValueChanged)
    {
        propertyLabelText.text = labelName;

        toggleInput.onValueChanged.RemoveAllListeners();
        toggleInput.isOn = (bool)currentValue;

        toggleInput.onValueChanged.AddListener(newValue =>
        {
            onValueChanged?.Invoke(newValue);
        });
    }
}
