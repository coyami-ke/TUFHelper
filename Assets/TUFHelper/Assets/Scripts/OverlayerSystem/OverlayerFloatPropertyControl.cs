using System;
using TMPro;
using UnityEngine;

public class OverlayerFloatPropertyControl : OverlayerPropertyControlField
{
    public TMP_InputField inputField;

    public float MinValue { get; private set; } = float.MinValue;
    public float MaxValue { get; private set; } = float.MaxValue;

    public void SetLimitations(float min, float max)
    {
        MinValue = min;
        MaxValue = max;
    }

    public override void BindProperty(string labelName, object currentValue, Action<object> onValueChanged)
    {
        if (propertyLabelText != null)
        {
            propertyLabelText.text = labelName;
        }

        float currentFloatValue = currentValue is float f ? f : 0f;

        currentFloatValue = Mathf.Clamp(currentFloatValue, MinValue, MaxValue);

        inputField.onValueChanged.RemoveAllListeners();
        inputField.onEndEdit.RemoveAllListeners();

        inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        inputField.text = currentFloatValue.ToString("F2");

        inputField.onEndEdit.AddListener(newText =>
        {
            if (float.TryParse(newText, out float parsedValue))
            {
                parsedValue = Mathf.Clamp(parsedValue, MinValue, MaxValue);

                onValueChanged?.Invoke(parsedValue);

                inputField.text = parsedValue.ToString("F2");

                currentFloatValue = parsedValue;
            }
            else
            {
                inputField.text = currentFloatValue.ToString("F2");
            }
        });
    }
}