using System;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class OverlayerFloatPropertyControl : OverlayerPropertyControlField
{
    public TMP_InputField inputField;
    public float MinValue { get; private set; } = float.MinValue;
    public float MaxValue { get; private set; } = float.MaxValue;

    private bool _isSelfModifying = false;

    public void SetLimitations(float min, float max) { MinValue = min; MaxValue = max; }

    public override void BindProperty(object target, string propertyName, string labelName, object currentValue, Action<object> onValueChanged)
    {
        if (targetSource is INotifyPropertyChanged oldNotify) oldNotify.PropertyChanged -= OnSourcePropertyChanged;

        targetSource = target;
        sourcePropertyName = propertyName;

        if (propertyLabelText != null) propertyLabelText.text = labelName;

        float currentFloatValue = currentValue is float f ? f : 0f;
        currentFloatValue = Mathf.Clamp(currentFloatValue, MinValue, MaxValue);

        inputField.onEndEdit.RemoveAllListeners();
        inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        inputField.text = currentFloatValue.ToString("F2");

        if (targetSource is INotifyPropertyChanged newNotify)
        {
            newNotify.PropertyChanged += OnSourcePropertyChanged;
        }

        inputField.onEndEdit.AddListener(newText =>
        {
            if (float.TryParse(newText, out float parsedValue))
            {
                _isSelfModifying = true;
                parsedValue = Mathf.Clamp(parsedValue, MinValue, MaxValue);
                onValueChanged?.Invoke(parsedValue);
                inputField.text = parsedValue.ToString("F2");
                _isSelfModifying = false;
            }
        });
    }

    protected override void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == sourcePropertyName && !_isSelfModifying)
        {
            var prop = targetSource.GetType().GetProperty(sourcePropertyName);
            if (prop != null)
            {
                float updatedValue = (float)prop.GetValue(targetSource);
                inputField.text = updatedValue.ToString("F2");
            }
        }
    }
}