using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ColorRampPointScript : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    public ColorRamp.RampPoint RampPoint { get; private set; }

    [SerializeField]
    private Image image;

    private RectTransform parentRect;
    private RectTransform rectTransform;

    public event Action<ColorRampPointScript, float> OnPointPositionChanged;
    public event Action<ColorRampPointScript> OnPointDoubleClicked;
    public event Action<ColorRampPointScript> OnPointSelected;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetPointInfo(ColorRamp.RampPoint point, Transform _parentTransform)
    {
        RampPoint = point;
        if (image != null)
        {
            image.color = point.color;
        }

        if (_parentTransform is RectTransform rt)
        {
            parentRect = rt;
        }

        UpdateVisualPosition();
    }

    public void UpdateVisualPosition()
    {
        if (RampPoint == null) return;

        float t = Mathf.Clamp01((float)RampPoint.position);

        rectTransform.anchorMin = new Vector2(t, 0.5f);
        rectTransform.anchorMax = new Vector2(t, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentRect == null || RampPoint == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            float trackWidth = parentRect.rect.width;
            float normalizedPos = Mathf.Clamp01((localPoint.x - parentRect.rect.xMin) / trackWidth);

            RampPoint.position = normalizedPos;
            UpdateVisualPosition();

            OnPointPositionChanged?.Invoke(this, normalizedPos);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2 || eventData.button == PointerEventData.InputButton.Right)
        {
            // Double click or right click to delete handle
            OnPointDoubleClicked?.Invoke(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Left click to select handle (e.g. open color picker)
            OnPointSelected?.Invoke(this);
        }
    }
}