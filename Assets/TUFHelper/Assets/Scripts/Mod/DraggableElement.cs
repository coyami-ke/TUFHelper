using UnityEngine;
using UnityEngine.EventSystems;
using TUFHelper;

public class DraggableElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private RectTransform parentRectTransform;
    private Canvas canvas;
    private bool isDragging = false;
    private Vector2 offset;

    public string saveID = "unknown";

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRectTransform = transform.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();

        //if (Main.Setting.OverlayerElementsPositions.ContainsKey(saveID))
        //{
        //    // Use anchoredPosition to ensure consistency with saving coordinates
        //    rectTransform.anchoredPosition = new Vector2(
        //        Main.Setting.OverlayerElementsPositions[saveID].X,
        //        Main.Setting.OverlayerElementsPositions[saveID].Y
        //    );
        //}
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (canvas == null) return;
        isDragging = true;

        // Calculate the mouse position relative to this object's IMMEDIATE PARENT
        // instead of the root canvas transform. This accounts for all parent scaling.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRectTransform,
            eventData.position, // Use the precise event eventData pointer position
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localMousePos
        );

        // Offset in anchored layout coordinates
        offset = rectTransform.anchoredPosition - localMousePos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        //Vector2 currentPos = rectTransform.anchoredPosition;

        //if (!Main.Setting.OverlayerElementsPositions.ContainsKey(saveID))
        //{
        //    Main.Setting.OverlayerElementsPositions[saveID] = new() { X = currentPos.x, Y = currentPos.y };
        //}
        //else
        //{
        //    Main.Setting.OverlayerElementsPositions[saveID].X = currentPos.x;
        //    Main.Setting.OverlayerElementsPositions[saveID].Y = currentPos.y;
        //}

        Main.Setting.Save(Main.ModEntry);
    }

    private void Update()
    {
        if (isDragging && canvas != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRectTransform,
                Input.mousePosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localMousePos
            );

            rectTransform.anchoredPosition = localMousePos + offset;
        }
    }
}