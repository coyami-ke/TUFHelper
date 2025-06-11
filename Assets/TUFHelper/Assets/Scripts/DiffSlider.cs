using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class DiffSlider : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public abstract float MaxWidth { get; protected set; }

    private int _selectedMinDiff;
    public int SelectedMinDiff
    {
        get => _selectedMinDiff;
        set
        {
            _selectedMinDiff = value;
            minDiffRect.anchoredPosition = new Vector2(minSliderPositionX + value * lengthStep, minDiffImage.rectTransform.anchoredPosition.y);
            minDiffImage.sprite = diffPairs[value].Sprite;
        }
    }

    private int _selectedMaxDiff;
    public int SelectedMaxDiff
    {
        get => _selectedMaxDiff;
        set
        {
            _selectedMaxDiff = value;
            maxDiffRect.anchoredPosition = new Vector2(minSliderPositionX + value * lengthStep, minDiffImage.rectTransform.anchoredPosition.y);
            maxDiffImage.sprite = diffPairs[value].Sprite;
        }
    }

    public RectTransform targetRectTransform;
    public Image minDiffImage, maxDiffImage;
    public float minSliderPositionX;

    private float lengthStep;
    private List<DiffSpritePair> diffPairs = new();

    private RectTransform minDiffRect, maxDiffRect;

    private bool isPointerHeld = false;
    private PointerEventData currentEventData;

    public int CountDiffs()
    {
        return diffPairs.Count;
    }
    public void Init(List<DiffSpritePair> diffPairs)
    {
        this.diffPairs = diffPairs;
        minDiffImage.sprite = diffPairs[0].Sprite;
        maxDiffImage.sprite = diffPairs.Last().Sprite;
        lengthStep = MaxWidth / diffPairs.Count;

        minDiffRect = minDiffImage.GetComponent<RectTransform>();
        maxDiffRect = maxDiffImage.GetComponent<RectTransform>();

        SelectedMinDiff = 0;
        SelectedMaxDiff = diffPairs.Count - 1;
    }

    private bool _moveMinSlider, _moveMaxSlider = false;
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
            float newX = localPoint.x + MaxWidth / 2;
            int step = Mathf.Clamp(Mathf.FloorToInt(newX / lengthStep), 0, diffPairs.Count - 1);

            if (Mathf.Abs(SelectedMinDiff - step) <= Mathf.Abs(SelectedMaxDiff - step) && step <= SelectedMaxDiff)
                _moveMinSlider = true;
            else if (step >= SelectedMinDiff)
                _moveMaxSlider = true;
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
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            float newX = localPoint.x + MaxWidth / 2;
            int step = Mathf.Clamp(Mathf.FloorToInt(newX / lengthStep), 0, diffPairs.Count - 1);

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
