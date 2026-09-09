using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class DiffSlider : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public abstract float MaxWidth { get; protected set; }

    [Header("UI References")]
    public RectTransform targetRectTransform;
    public Image minDiffImage, maxDiffImage;
    public float minSliderPositionX;

    [Header("Smoothing Settings")]
    [Tooltip("Speed of handle interpolation. Higher values mean faster tracking.")]
    public float smoothSpeed = 25f;

    private int _selectedMinDiff;
    public int SelectedMinDiff
    {
        get => _selectedMinDiff;
        set
        {
            value = Mathf.Clamp(value, 0, diffPairs.Count - 1);
            if (_selectedMinDiff == value && _minTargetPos.x > 0) return;

            _selectedMinDiff = value;
            _minTargetPos = new Vector2(minSliderPositionX + value * lengthStep, minDiffRect.anchoredPosition.y);
            if (diffPairs.Count > value && minDiffImage != null)
            {
                minDiffImage.sprite = diffPairs[value].Sprite;
            }
        }
    }

    private int _selectedMaxDiff;
    public int SelectedMaxDiff
    {
        get => _selectedMaxDiff;
        set
        {
            value = Mathf.Clamp(value, 0, diffPairs.Count - 1);
            if (_selectedMaxDiff == value && _maxTargetPos.x > 0) return;

            _selectedMaxDiff = value;
            _maxTargetPos = new Vector2(minSliderPositionX + value * lengthStep, maxDiffRect.anchoredPosition.y);
            if (diffPairs.Count > value && maxDiffImage != null)
            {
                maxDiffImage.sprite = diffPairs[value].Sprite;
            }
        }
    }

    private float lengthStep;
    public List<DiffSpritePair> diffPairs { get; private set; } = new();

    private RectTransform minDiffRect, maxDiffRect;

    private Vector2 _minTargetPos;
    private Vector2 _maxTargetPos;

    private bool isPointerHeld = false;
    private PointerEventData currentEventData;
    private bool _moveMinSlider, _moveMaxSlider;

    public int CountDiffs()
    {
        return diffPairs.Count;
    }

    public void Init(List<DiffSpritePair> diffPairs)
    {
        if (diffPairs == null || diffPairs.Count == 0) return;

        this.diffPairs = diffPairs;
        lengthStep = diffPairs.Count > 1 ? MaxWidth / (diffPairs.Count) : MaxWidth;

        minDiffRect = minDiffImage.GetComponent<RectTransform>();
        maxDiffRect = maxDiffImage.GetComponent<RectTransform>();

        // Set initial discrete indices
        SelectedMinDiff = 0;
        SelectedMaxDiff = diffPairs.Count - 1;

        minDiffImage.sprite = diffPairs[0].Sprite;
        maxDiffImage.sprite = diffPairs.Last().Sprite;

        // Snap positions directly on initialization without Lerp lag
        //_minTargetPos = new Vector2(minSliderPositionX, minDiffRect.anchoredPosition.y);
        //_maxTargetPos = new Vector2(minSliderPositionX + MaxWidth, maxDiffRect.anchoredPosition.y);

        //minDiffRect.anchoredPosition = _minTargetPos;
        //maxDiffRect.anchoredPosition = _maxTargetPos;
    }

    private void Update()
    {
        if (minDiffRect != null)
        {
            minDiffRect.anchoredPosition = Vector2.Lerp(minDiffRect.anchoredPosition, _minTargetPos, Time.deltaTime * smoothSpeed);
        }

        if (maxDiffRect != null)
        {
            maxDiffRect.anchoredPosition = Vector2.Lerp(maxDiffRect.anchoredPosition, _maxTargetPos, Time.deltaTime * smoothSpeed);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UpdateSliderValue(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            float newX = localPoint.x + MaxWidth / 2f;
            int step = Mathf.Clamp(Mathf.RoundToInt(newX / lengthStep), 0, diffPairs.Count - 1);

            _moveMinSlider = false;
            _moveMaxSlider = false;

            if (Mathf.Abs(SelectedMinDiff - step) < Mathf.Abs(SelectedMaxDiff - step))
            {
                if (step <= SelectedMaxDiff) _moveMinSlider = true;
            }
            else if (Mathf.Abs(SelectedMinDiff - step) > Mathf.Abs(SelectedMaxDiff - step))
            {
                if (step >= SelectedMinDiff) _moveMaxSlider = true;
            }
            else
            {
                // Equal distance edge-case: move min if clicking left half, max if clicking right half
                if (step <= SelectedMinDiff) _moveMinSlider = true;
                else _moveMaxSlider = true;
            }
        }

        isPointerHeld = true;
        currentEventData = eventData;
        StartCoroutine(UpdateWhileHeld());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerHeld = false;
        _moveMaxSlider = false;
        _moveMinSlider = false;
        OnMouseUp();
    }

    private IEnumerator UpdateWhileHeld()
    {
        while (isPointerHeld)
        {
            UpdateSliderValue(currentEventData);
            yield return null;
        }
    }

    private void UpdateSliderValue(PointerEventData eventData)
    {
        if (eventData == null || diffPairs.Count == 0) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            float newX = localPoint.x + MaxWidth / 2f;
            int step = Mathf.Clamp(Mathf.RoundToInt(newX / lengthStep), 0, diffPairs.Count - 1);

            if (_moveMinSlider && step <= SelectedMaxDiff)
            {
                SelectedMinDiff = step;
            }
            else if (_moveMaxSlider && step >= SelectedMinDiff)
            {
                SelectedMaxDiff = step;
            }
        }
    }

    public virtual void OnMouseUp() { }
}

public class DiffSpritePair
{
    public string Name { get; set; }
    public Sprite Sprite { get; set; }

    public DiffSpritePair(string name, Sprite sprite)
    {
        Name = name;
        Sprite = sprite;
    }
}