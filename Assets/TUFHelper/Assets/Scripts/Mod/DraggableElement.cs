using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool isDragging = false;
    private Vector2 offset;

    public string saveID = "unknown";

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (Main.Setting.OverlayerElementsPositions.ContainsKey(saveID))
        {
            rectTransform.localPosition = new(Main.Setting.OverlayerElementsPositions[saveID].X, Main.Setting.OverlayerElementsPositions[saveID].Y);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localMousePos
        );
        offset = rectTransform.localPosition - (Vector3)localMousePos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        if (!Main.Setting.OverlayerElementsPositions.ContainsKey(saveID)) Main.Setting.OverlayerElementsPositions[saveID] = new() { X = rectTransform.localPosition.x, Y = rectTransform.localPosition.y };
        else
        {
            Main.Setting.OverlayerElementsPositions[saveID].X = rectTransform.localPosition.x;
            Main.Setting.OverlayerElementsPositions[saveID].Y = rectTransform.localPosition.y;
        }
    }


    private void Update()
    {
        if (isDragging)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localMousePos
            );
            rectTransform.localPosition = localMousePos + offset;
        }
    }
}
