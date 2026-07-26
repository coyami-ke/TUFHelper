using System;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class OverlayerStringPropertyControl : OverlayerPropertyControlField
{
    public TMP_InputField inputField;

    private bool _isSelfModifying = false;

    public override void BindProperty(object target, string propertyName, string labelName, object currentValue, Action<object> onValueChanged)
    {
        if (targetSource is INotifyPropertyChanged oldNotify)
        {
            oldNotify.PropertyChanged -= OnSourcePropertyChanged;
        }

        targetSource = target;
        sourcePropertyName = propertyName;

        if (propertyLabelText != null)
        {
            propertyLabelText.text = labelName;
        }

        string stringValue = currentValue?.ToString() ?? string.Empty;

        inputField.onEndEdit.RemoveAllListeners();
        inputField.contentType = TMP_InputField.ContentType.Standard;
        inputField.text = stringValue;

        if (targetSource is INotifyPropertyChanged newNotify)
        {
            newNotify.PropertyChanged += OnSourcePropertyChanged;
        }

        inputField.onEndEdit.AddListener(newText =>
        {
            _isSelfModifying = true;

            onValueChanged?.Invoke(newText);

            _isSelfModifying = false;
        });
    }

    protected override void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == sourcePropertyName && !_isSelfModifying)
        {
            var prop = targetSource?.GetType().GetProperty(sourcePropertyName);
            if (prop != null)
            {
                string updatedValue = prop.GetValue(targetSource)?.ToString() ?? string.Empty;

                if (inputField.text != updatedValue)
                {
                    inputField.text = updatedValue;
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (targetSource is INotifyPropertyChanged notify)
        {
            notify.PropertyChanged -= OnSourcePropertyChanged;
        }
    }
}