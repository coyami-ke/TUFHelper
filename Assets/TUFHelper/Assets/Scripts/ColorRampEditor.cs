using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ColorRampEditor : OverlayerPropertyControlField, IPointerClickHandler
{
    [Header("UI & Prefab References")]
    public ColorRampScript colorRampView;
    public RectTransform handlesTrackArea;
    public GameObject pointPrefab;

    public TMP_InputField pointNumberField;
    public TMP_InputField positionField;
    public TMP_Dropdown interpolationDropdown;

    public Button selectedColor;

    public TMP_InputField colorRField, colorGField, colorBField;

    public ColorPicker colorPicker;

    [Header("Settings")]
    public int maxPoints = 8;

    private ColorRamp currentRamp = new();
    private readonly List<ColorRampPointScript> spawnedHandles = new();

    private ColorRampPointScript selectedHandle;
    private Action<object> _onValueChanged;
    private bool _isSelfModifying = false;

    protected void OnEnable()
    {
        SetupControlListeners();
    }

    protected void OnDisable()
    {
        RemoveControlListeners();
    }

    protected void OnRectTransformDimensionsChange()
    {
        RepositionAllHandles();
        UpdateRampView();
    }

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

        if (currentValue is ColorRamp ramp)
        {
            currentRamp = ramp;
        }

        RebuildUI();

        if (targetSource is INotifyPropertyChanged newNotify)
        {
            newNotify.PropertyChanged += OnSourcePropertyChanged;
        }
    }

    public void RebuildUI()
    {
        foreach (var handle in spawnedHandles)
        {
            if (handle != null)
            {
                handle.OnPointPositionChanged -= OnPointMoved;
                handle.OnPointDoubleClicked -= OnPointDeleted;
                Destroy(handle.gameObject);
            }
        }
        spawnedHandles.Clear();

        if (currentRamp == null || currentRamp.points == null) return;

        currentRamp.points.Sort((a, b) => a.position.CompareTo(b.position));

        Transform parentTransform = handlesTrackArea != null ? handlesTrackArea : transform;

        foreach (var point in currentRamp.points)
        {
            GameObject handleObj = Instantiate(pointPrefab, parentTransform, false);
            ColorRampPointScript pointScript = handleObj.GetComponent<ColorRampPointScript>();

            if (pointScript != null)
            {
                pointScript.SetPointInfo(point, parentTransform as RectTransform);

                pointScript.OnPointPositionChanged += OnPointMoved;
                pointScript.OnPointDoubleClicked += OnPointDeleted;

                spawnedHandles.Add(pointScript);
            }
        }

        if (spawnedHandles.Count > 0)
        {
            SelectPoint(spawnedHandles[0]);
        }
        else
        {
            ClearSelection();
        }

        UpdateRampView();
    }

    private void RepositionAllHandles()
    {
        RectTransform track = handlesTrackArea != null ? handlesTrackArea : GetComponent<RectTransform>();
        if (track == null || track.rect.width <= 0) return;

        foreach (var handle in spawnedHandles)
        {
            if (handle != null)
            {
                handle.SetPointInfo(handle.RampPoint, track);
            }
        }
    }

    private readonly Vector3[] _corners1 = new Vector3[4];
    private readonly Vector3[] _corners2 = new Vector3[4];

    public bool Overlaps(RectTransform rectTrans1, RectTransform rectTrans2)
    {
        if (rectTrans1 == null || rectTrans2 == null) return false;

        rectTrans1.GetWorldCorners(_corners1);
        rectTrans2.GetWorldCorners(_corners2);

        Rect rect1 = new Rect(_corners1[0].x, _corners1[0].y, _corners1[2].x - _corners1[0].x, _corners1[2].y - _corners1[0].y);
        Rect rect2 = new Rect(_corners2[0].x, _corners2[0].y, _corners2[2].x - _corners2[0].x, _corners2[2].y - _corners2[0].y);

        return rect1.Overlaps(rect2);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerPress != gameObject && (handlesTrackArea != null && eventData.pointerPress != handlesTrackArea.gameObject))
            return;

        // 2. Guard against max points limit
        if (currentRamp.points.Count >= maxPoints)
            return;

        RectTransform track = handlesTrackArea != null ? handlesTrackArea : GetComponent<RectTransform>();

        if (track.rect.width <= 0 || track.rect.height <= 0)
            return;

        if (!RectTransformUtility.RectangleContainsScreenPoint(track, eventData.position, eventData.pressEventCamera))
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            track,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            float normalizedPos = Mathf.Clamp01((localPoint.x - track.rect.xMin) / track.rect.width);
            Color sampledColor = EvaluateRampColor(normalizedPos);

            var newPoint = new ColorRamp.RampPoint
            {
                position = normalizedPos,
                color = sampledColor,
                interpolation = ColorRamp.InterpolationType.Linear
            };

            currentRamp.points.Add(newPoint);
            RebuildUI();

            var newHandle = spawnedHandles.Find(h => h.RampPoint == newPoint);
            if (newHandle != null)
            {
                SelectPoint(newHandle);
            }

            NotifyValueChanged();
        }
    }

    private void OnPointMoved(ColorRampPointScript pointScript, float newPosition)
    {
        currentRamp.points.Sort((a, b) => a.position.CompareTo(b.position));
        SelectPoint(pointScript);
        UpdateRampView();
        NotifyValueChanged();
    }

    private void OnPointDeleted(ColorRampPointScript pointScript)
    {
        if (currentRamp.points.Count <= 2) return;

        currentRamp.points.Remove(pointScript.RampPoint);
        RebuildUI();
        NotifyValueChanged();
    }

    private void SelectPoint(ColorRampPointScript handle)
    {
        selectedHandle = handle;
        UpdateInspectorFields();

        if (selectedHandle != null && selectedHandle.RampPoint != null)
        {
            if (colorPicker != null)
            {
                colorPicker.SelectedColor = selectedHandle.RampPoint.color;
            }
        }
    }

    private void ClearSelection()
    {
        selectedHandle = null;
        if (pointNumberField != null) pointNumberField.text = "-";
        if (positionField != null) positionField.text = "0.00";
        if (interpolationDropdown != null) interpolationDropdown.value = 0;

        if (colorRField != null) colorRField.text = "0.00";
        if (colorGField != null) colorGField.text = "0.00";
        if (colorBField != null) colorBField.text = "0.00";

        if (selectedColor != null && selectedColor.image != null)
        {
            selectedColor.image.color = Color.clear;
        }
    }

    private void UpdateInspectorFields()
    {
        if (selectedHandle == null || selectedHandle.RampPoint == null)
        {
            ClearSelection();
            return;
        }

        int index = currentRamp.points.IndexOf(selectedHandle.RampPoint);

        if (pointNumberField != null)
        {
            pointNumberField.text = (index + 1).ToString();
        }

        if (positionField != null)
        {
            positionField.text = selectedHandle.RampPoint.position.ToString("F2");
        }

        if (interpolationDropdown != null)
        {
            interpolationDropdown.value = (int)selectedHandle.RampPoint.interpolation;
        }

        Color c = selectedHandle.RampPoint.color;

        if (colorRField != null) colorRField.text = c.r.ToString("F2");
        if (colorGField != null) colorGField.text = c.g.ToString("F2");
        if (colorBField != null) colorBField.text = c.b.ToString("F2");

        if (selectedColor != null && selectedColor.image != null)
        {
            selectedColor.image.color = c;
        }
    }

    private void SetupControlListeners()
    {
        if (pointNumberField != null)
            pointNumberField.onEndEdit.AddListener(OnPointNumberFieldEndEdit);

        if (positionField != null)
            positionField.onEndEdit.AddListener(OnPositionFieldEndEdit);

        if (interpolationDropdown != null)
            interpolationDropdown.onValueChanged.AddListener(OnInterpolationChanged);

        if (colorRField != null) colorRField.onEndEdit.AddListener(OnRgbInputEndEdit);
        if (colorGField != null) colorGField.onEndEdit.AddListener(OnRgbInputEndEdit);
        if (colorBField != null) colorBField.onEndEdit.AddListener(OnRgbInputEndEdit);

        if (selectedColor != null)
            selectedColor.onClick.AddListener(ToggleColorPicker);

        if (colorPicker != null)
            colorPicker.ColorChanged += OnColorPickerChanged;
    }

    private void RemoveControlListeners()
    {
        if (pointNumberField != null)
            pointNumberField.onEndEdit.RemoveListener(OnPointNumberFieldEndEdit);

        if (positionField != null)
            positionField.onEndEdit.RemoveListener(OnPositionFieldEndEdit);

        if (interpolationDropdown != null)
            interpolationDropdown.onValueChanged.RemoveListener(OnInterpolationChanged);

        if (colorRField != null) colorRField.onEndEdit.RemoveListener(OnRgbInputEndEdit);
        if (colorGField != null) colorGField.onEndEdit.RemoveListener(OnRgbInputEndEdit);
        if (colorBField != null) colorBField.onEndEdit.RemoveListener(OnRgbInputEndEdit);

        if (selectedColor != null)
            selectedColor.onClick.RemoveListener(ToggleColorPicker);

        if (colorPicker != null)
            colorPicker.ColorChanged -= OnColorPickerChanged;
    }
    private void OnColorPickerChanged(object sender, ColorPickerEventArgs e)
    {
        ApplyNewColorToSelected(e.Color, updatePicker: false);
    }

    private void OnRgbInputEndEdit(string text)
    {
        if (selectedHandle == null || selectedHandle.RampPoint == null) return;

        float r = ParseNormalizedColorField(colorRField, selectedHandle.RampPoint.color.r);
        float g = ParseNormalizedColorField(colorGField, selectedHandle.RampPoint.color.g);
        float b = ParseNormalizedColorField(colorBField, selectedHandle.RampPoint.color.b);

        Color newColor = new Color(r, g, b, selectedHandle.RampPoint.color.a);
        ApplyNewColorToSelected(newColor, updatePicker: true);
    }

    private float ParseNormalizedColorField(TMP_InputField field, float fallbackVal)
    {
        if (field != null && float.TryParse(field.text, out float val))
        {
            return Mathf.Clamp01(val);
        }
        return fallbackVal;
    }

    private void ApplyNewColorToSelected(Color newColor, bool updatePicker)
    {
        if (selectedHandle == null || selectedHandle.RampPoint == null) return;

        selectedHandle.RampPoint.color = newColor;

        if (updatePicker && colorPicker != null)
        {
            colorPicker.SelectedColor = newColor;
        }

        RectTransform track = handlesTrackArea != null ? handlesTrackArea : GetComponent<RectTransform>();
        selectedHandle.SetPointInfo(selectedHandle.RampPoint, track as RectTransform);

        UpdateInspectorFields();

        UpdateRampView();
        NotifyValueChanged();
    }

    private void ToggleColorPicker()
    {
        if (colorPicker != null)
        {
            bool isActive = !colorPicker.gameObject.activeSelf;
            colorPicker.gameObject.SetActive(isActive);

            if (isActive && selectedHandle != null && selectedHandle.RampPoint != null)
            {
                colorPicker.SelectedColor = selectedHandle.RampPoint.color;
            }
        }
    }

    private void OnPointNumberFieldEndEdit(string text)
    {
        if (spawnedHandles.Count == 0) return;

        if (int.TryParse(text, out int targetNumber))
        {
            int clampedIndex = Mathf.Clamp(targetNumber - 1, 0, spawnedHandles.Count - 1);

            var targetHandle = spawnedHandles.Find(h => currentRamp.points.IndexOf(h.RampPoint) == clampedIndex);

            if (targetHandle != null)
            {
                SelectPoint(targetHandle);
            }
            else
            {
                UpdateInspectorFields();
            }
        }
        else
        {
            UpdateInspectorFields();
        }
    }

    private void OnPositionFieldEndEdit(string text)
    {
        if (selectedHandle == null || selectedHandle.RampPoint == null) return;

        if (float.TryParse(text, out float newPos))
        {
            newPos = Mathf.Clamp01(newPos);
            selectedHandle.RampPoint.position = newPos;

            RectTransform track = handlesTrackArea != null ? handlesTrackArea : GetComponent<RectTransform>();
            selectedHandle.SetPointInfo(selectedHandle.RampPoint, track as RectTransform);

            currentRamp.points.Sort((a, b) => a.position.CompareTo(b.position));
            RebuildUI();
            NotifyValueChanged();
        }
    }

    private void OnInterpolationChanged(int index)
    {
        if (selectedHandle == null || selectedHandle.RampPoint == null) return;

        selectedHandle.RampPoint.interpolation = (ColorRamp.InterpolationType)index;
        UpdateRampView();
        NotifyValueChanged();
    }

    private void NotifyValueChanged()
    {
        _isSelfModifying = true;
        _onValueChanged?.Invoke(currentRamp);
        _isSelfModifying = false;
    }

    private void UpdateRampView()
    {
        if (colorRampView != null)
        {
            colorRampView.ApplyRampToImage(currentRamp);
        }
    }

    protected override void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == sourcePropertyName && !_isSelfModifying)
        {
            var prop = targetSource?.GetType().GetProperty(sourcePropertyName);
            if (prop != null)
            {
                var val = prop.GetValue(targetSource);
                if (val is ColorRamp newRamp)
                {
                    currentRamp = newRamp;
                    RebuildUI();
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

    private Color EvaluateRampColor(float position)
    {
        if (currentRamp.points == null || currentRamp.points.Count == 0) return Color.white;
        if (position <= currentRamp.points[0].position) return currentRamp.points[0].color;
        if (position >= currentRamp.points[^1].position) return currentRamp.points[^1].color;

        for (int i = 0; i < currentRamp.points.Count - 1; i++)
        {
            var p1 = currentRamp.points[i];
            var p2 = currentRamp.points[i + 1];

            if (position >= p1.position && position <= p2.position)
            {
                float range = p2.position - p1.position;
                if (range <= 0.0001f) return p1.color;

                float t = (position - p1.position) / range;
                return Color.Lerp(p1.color, p2.color, t);
            }
        }

        return Color.white;
    }
}