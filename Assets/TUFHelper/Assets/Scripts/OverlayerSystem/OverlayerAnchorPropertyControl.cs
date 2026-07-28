using System;
using System.ComponentModel;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public enum Anchor
{
    LeftTop, MiddleTop, RightTop,
    LeftMiddle, Center, RightMiddle,
    LeftBottom, MiddleBottom, RightBottom,
}

public class OverlayerAnchorPropertyControl : OverlayerPropertyControlField
{
    [Header("Current Value Display")]
    public Button currentAnchorButton;
    public AnchorIconGraphic currentAnchorIcon;
    public TMP_Text currentAnchorLabel;

    [Header("Popup Grid Picker (Optional / Toggleable)")]
    public GameObject anchorPickerPopup;

    private Anchor _currentAnchor = Anchor.Center;
    private Action<object> _onValueChanged;
    private bool _isSelfModifying = false;

    public override void BindProperty(object target, string propertyName, string labelName, object currentValue, Action<object> onValueChanged)
    {
        if (targetSource is INotifyPropertyChanged oldNotify)
        {
            oldNotify.PropertyChanged -= OnSourcePropertyChanged;
        }

        targetSource = target;
        sourcePropertyName = propertyName;
        _onValueChanged = onValueChanged;

        if (propertyLabelText != null)
        {
            propertyLabelText.text = labelName;
        }

        if (currentValue is Anchor anchorValue)
        {
            _currentAnchor = anchorValue;
        }
        else if (currentValue != null && Enum.TryParse(currentValue.ToString(), out Anchor parsedAnchor))
        {
            _currentAnchor = parsedAnchor;
        }

        currentAnchorButton.onClick.RemoveAllListeners();
        currentAnchorButton.onClick.AddListener(OnMainAnchorButtonClicked);

        UpdateUIState();

        if (targetSource is INotifyPropertyChanged newNotify)
        {
            newNotify.PropertyChanged += OnSourcePropertyChanged;
        }
    }

    public void SetAnchor(Anchor newAnchor)
    {
        _isSelfModifying = true;
        _currentAnchor = newAnchor;

        _onValueChanged?.Invoke(_currentAnchor);
        UpdateUIState();

        anchorPickerPopup.SetActive(false);

        _isSelfModifying = false;
    }

    public void OnMainAnchorButtonClicked()
    {
        bool willBeActive = !anchorPickerPopup.activeSelf;
        anchorPickerPopup.SetActive(willBeActive);

        if (willBeActive)
        {
            transform.SetAsLastSibling();

            anchorPickerPopup.transform.SetAsLastSibling();
        }
    }

    private void UpdateUIState()
    {
        currentAnchorIcon.anchor = _currentAnchor;
        currentAnchorIcon.SetVerticesDirty(); 

        currentAnchorLabel.text = _currentAnchor.ToString();
    }

    protected override void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == sourcePropertyName && !_isSelfModifying)
        {
            var prop = targetSource?.GetType().GetProperty(sourcePropertyName);
            if (prop != null)
            {
                var val = prop.GetValue(targetSource);
                if (val is Anchor newAnchor)
                {
                    _currentAnchor = newAnchor;
                    UpdateUIState();
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
    public void OnSelectLeftTop() => SetAnchor(Anchor.LeftTop);
    public void OnSelectMiddleTop() => SetAnchor(Anchor.MiddleTop);
    public void OnSelectRightTop() => SetAnchor(Anchor.RightTop);

    public void OnSelectLeftMiddle() => SetAnchor(Anchor.LeftMiddle);
    public void OnSelectCenter() => SetAnchor(Anchor.Center);
    public void OnSelectRightMiddle() => SetAnchor(Anchor.RightMiddle);

    public void OnSelectLeftBottom() => SetAnchor(Anchor.LeftBottom);
    public void OnSelectMiddleBottom() => SetAnchor(Anchor.MiddleBottom);
    public void OnSelectRightBottom() => SetAnchor(Anchor.RightBottom);
}