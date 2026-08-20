using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Version of <see cref="ScrollRect"/> that supports responsive smooth scrolling.
/// </summary>
public class SmoothScrollRect : ScrollRect
{
    [field: SerializeField] public bool SmoothScrolling { get; set; } = true;
    [field: SerializeField] public float SmoothScrollTime { get; set; } = 0.25f;
    [field: SerializeField] public float SensitivityMultiplier { get; set; } = 332.0f / 1;
    [field: SerializeField] public Ease ScrollEase { get; set; } = Ease.OutCubic;

    private Vector2 _targetPosition;

    protected override void Awake()
    {
        base.Awake();
        _targetPosition = normalizedPosition;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _targetPosition = normalizedPosition;
    }

    public override void OnScroll(PointerEventData data)
    {
        if (!IsActive())
            return;

        if (SmoothScrolling)
        {
            Vector2 positionBefore = normalizedPosition;
            base.OnScroll(data);
            Vector2 delta = (normalizedPosition - positionBefore) * SensitivityMultiplier;

            normalizedPosition = positionBefore;

            if (!DOTween.IsTweening(this))
            {
                _targetPosition = normalizedPosition;
            }

            _targetPosition += delta;
            _targetPosition.x = Mathf.Clamp01(_targetPosition.x);
            _targetPosition.y = Mathf.Clamp01(_targetPosition.y);

            this.DOKill();
            this.DONormalizedPos(_targetPosition, SmoothScrollTime).SetEase(ScrollEase);
        }
        else
        {
            base.OnScroll(data);
        }
    }
}