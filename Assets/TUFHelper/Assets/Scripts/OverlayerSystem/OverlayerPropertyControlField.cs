using UnityEngine;
using System;

public abstract class OverlayerPropertyControlField : MonoBehaviour
{
    public TMPro.TextMeshProUGUI propertyLabelText;

    protected object targetSource;
    protected string sourcePropertyName;

    public abstract void BindProperty(object target, string propertyName, string labelName, object currentValue, Action<object> onValueChanged);
    protected abstract void OnSourcePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e);

    protected virtual void OnDestroy()
    {
        if (targetSource is System.ComponentModel.INotifyPropertyChanged notifySource)
        {
            notifySource.PropertyChanged -= OnSourcePropertyChanged;
        }
    }
}