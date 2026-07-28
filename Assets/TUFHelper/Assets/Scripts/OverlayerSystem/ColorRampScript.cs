using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteAlways]
public class ColorRampScript : MonoBehaviour
{
    private static readonly int PointCountID = Shader.PropertyToID("_PointCount");
    private static readonly int PositionsID = Shader.PropertyToID("_RampPositions");
    private static readonly int ColorsID = Shader.PropertyToID("_RampColors");
    private static readonly int InterpID = Shader.PropertyToID("_RampInterpolations");

    private const int MaxPoints = 8;

    private Image image;
    private Material instanceMaterial;

    [SerializeField]
    private Shader shader;

    private void Awake()
    {
        image = GetComponent<Image>();
        EnsureMaterialInstance();
    }

    private void Start()
    {
        ApplyRampToImage(new());
    }

    private void OnDestroy()
    {
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }

    public void ApplyRampToImage(ColorRamp ramp)
    {
        if (ramp == null || ramp.points == null) return;

        EnsureMaterialInstance();
        if (instanceMaterial == null) return;

        ramp.points.Sort((a, b) => a.position.CompareTo(b.position));

        int count = Mathf.Min(ramp.points.Count, MaxPoints);

        float[] positions = new float[MaxPoints];
        Vector4[] colors = new Vector4[MaxPoints];
        float[] interps = new float[MaxPoints];

        for (int i = 0; i < count; i++)
        {
            var pt = ramp.points[i];
            positions[i] = Mathf.Clamp01(pt.position);
            colors[i] = pt.color;
            interps[i] = (float)pt.interpolation;
        }

        instanceMaterial.SetInt(PointCountID, count);
        instanceMaterial.SetFloatArray(PositionsID, positions);
        instanceMaterial.SetVectorArray(ColorsID, colors);
        instanceMaterial.SetFloatArray(InterpID, interps);
    }

    private void EnsureMaterialInstance()
    {
        if (instanceMaterial == null)
        {
            instanceMaterial = new Material(shader)
            {
                name = $"{gameObject.name}_ColorRamp_Inst"
            };

            image.material = instanceMaterial;
        }
    }
}