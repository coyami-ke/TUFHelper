using DG.Tweening;
using TUFHelper;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ResizableScreen : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minScale = 0.2f;
    [SerializeField] private float maxScale = 2.0f;

    private RectTransform rectTransform;
    private float currentScale = 1.0f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateResolution();
    }

    private void Update()
    {
        HandleScrollZoom();
    }

    private void HandleScrollZoom()
    {
        float scrollDelta = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scrollDelta) > 0.01f)
        {
            currentScale += scrollDelta * zoomSpeed;
            currentScale = Mathf.Clamp(currentScale, minScale, maxScale);

            ApplyScale();
        }
    }

    public void UpdateResolution()
    {
        float referenceHeight = 1000f;

        float aspectRatio = (float)Screen.width / Screen.height;

        Vector2 simulatedAdofaiCanvasSize = new Vector2(referenceHeight * aspectRatio, referenceHeight);

        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = simulatedAdofaiCanvasSize / 2.125f;
        rectTransform.anchoredPosition = Vector2.zero;

        ApplyScale();
    }

    private void ApplyScale()
    {
        rectTransform.DOKill();
        rectTransform.DOScale(Vector3.one * currentScale, 0.25f).SetEase(Ease.OutExpo);
    }
}