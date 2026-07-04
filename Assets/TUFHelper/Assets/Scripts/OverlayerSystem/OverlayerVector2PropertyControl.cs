using System;
using System.ComponentModel;
using System.Numerics;
using TMPro;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

public class OverlayerVector2PropertyControl : OverlayerPropertyControlField
{
    public TextMeshProUGUI label;
    public TMP_InputField xField;
    public TMP_InputField yField;

    private bool _isSelfModifying = false;

    public override void BindProperty(object target, string propertyName, string labelName, object currentValue, Action<object> onValueChanged)
    {
        if (targetSource is INotifyPropertyChanged oldNotify) oldNotify.PropertyChanged -= OnSourcePropertyChanged;

        targetSource = target;
        sourcePropertyName = propertyName;

        if (propertyLabelText != null) propertyLabelText.text = labelName;
        if (label != null) label.text = labelName;

        Vector2 currentVector = currentValue is Vector2 vec ? vec : Vector2.Zero;

        xField.onEndEdit.RemoveAllListeners();
        yField.onEndEdit.RemoveAllListeners();

        xField.contentType = TMP_InputField.ContentType.DecimalNumber;
        yField.contentType = TMP_InputField.ContentType.DecimalNumber;

        xField.text = currentVector.X.ToString("F2");
        yField.text = currentVector.Y.ToString("F2");

        if (targetSource is INotifyPropertyChanged newNotify)
        {
            newNotify.PropertyChanged += OnSourcePropertyChanged;
        }

        xField.onEndEdit.AddListener(newText =>
        {
            if (float.TryParse(newText, out float parsedX))
            {
                _isSelfModifying = true;
                Vector2 current = GetCurrentVector();
                current.X = parsedX;
                onValueChanged?.Invoke(current);
                xField.text = parsedX.ToString("F2");
                _isSelfModifying = false;
            }
        });

        yField.onEndEdit.AddListener(newText =>
        {
            if (float.TryParse(newText, out float parsedY))
            {
                _isSelfModifying = true;
                Vector2 current = GetCurrentVector();
                current.Y = parsedY;
                onValueChanged?.Invoke(current);
                yField.text = parsedY.ToString("F2");
                _isSelfModifying = false;
            }
        });
    }

    private Vector2 GetCurrentVector()
    {
        var prop = targetSource.GetType().GetProperty(sourcePropertyName);
        return prop != null ? (Vector2)prop.GetValue(targetSource) : Vector2.Zero;
    }

    protected override void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == sourcePropertyName && !_isSelfModifying)
        {
            Vector2 updatedVector = GetCurrentVector();

            xField.text = updatedVector.X.ToString("F2");
            yField.text = updatedVector.Y.ToString("F2");
        }
    }
}