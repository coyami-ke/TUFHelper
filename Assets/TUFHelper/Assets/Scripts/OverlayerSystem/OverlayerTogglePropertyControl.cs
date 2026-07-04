using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class OverlayerTogglePropertyControl : OverlayerPropertyControlField
{
    public Toggle toggleInput;

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

        toggleInput.onValueChanged.RemoveAllListeners();
        toggleInput.isOn = currentValue is bool b && b;

        if (targetSource is INotifyPropertyChanged newNotify)
        {
            newNotify.PropertyChanged += OnSourcePropertyChanged;
        }

        toggleInput.onValueChanged.AddListener(newValue =>
        {
            _isSelfModifying = true;
            onValueChanged?.Invoke(newValue);
            _isSelfModifying = false;
        });
    }

    protected override void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == sourcePropertyName && !_isSelfModifying)
        {
            var prop = targetSource.GetType().GetProperty(sourcePropertyName);
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                bool updatedValue = (bool)prop.GetValue(targetSource);

                toggleInput.onValueChanged.RemoveAllListeners();
                toggleInput.isOn = updatedValue;

                toggleInput.onValueChanged.AddListener(newValue =>
                {
                    _isSelfModifying = true;
                    var currentProp = targetSource.GetType().GetProperty(sourcePropertyName);
                    if (currentProp != null)
                    {
                        var propValue = currentProp.GetValue(targetSource);
                        System.Reflection.PropertyInfo propertyInfo = targetSource.GetType().GetProperty(sourcePropertyName);
                    }
                    _isSelfModifying = false;
                });
            }
        }
    }
}