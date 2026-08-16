using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class UIProximityTracker : MonoBehaviour
{
    [SerializeField]
    private Material baseMaterial;

    private Material instancedMaterial;
    private RectTransform rectTransform;
    private Canvas canvas;

    private static readonly int MouseUVProperty = Shader.PropertyToID("_MouseUV");

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        Image img = GetComponent<Image>();

        if (baseMaterial != null)
        {
            instancedMaterial = new Material(baseMaterial);
            img.material = instancedMaterial;
        }
        else
        {
            instancedMaterial = img.material;
        }
    }

    private void Update()
    {
        if (instancedMaterial == null) return;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        Camera cam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? null
            : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, cam, out Vector2 localPoint))
        {
            Rect rect = rectTransform.rect;

            float uvX = (localPoint.x - rect.x) / rect.width;
            float uvY = (localPoint.y - rect.y) / rect.height;

            instancedMaterial.SetVector(MouseUVProperty, new Vector4(uvX, uvY, 0, 0));
        }
    }

    private void OnDestroy()
    {
        if (instancedMaterial != null && Application.isPlaying)
        {
            Destroy(instancedMaterial);
        }
    }
}