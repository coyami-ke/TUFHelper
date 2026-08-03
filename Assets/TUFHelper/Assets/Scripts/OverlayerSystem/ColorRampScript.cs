using TUFHelper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteAlways]
public class ColorRampScript : UIBehaviour, IMaterialModifier
{
    private static readonly int PointCountID = Shader.PropertyToID("_PointCount");
    private static readonly int PositionsID = Shader.PropertyToID("_RampPositions");
    private static readonly int ColorsID = Shader.PropertyToID("_RampColors");
    private static readonly int InterpID = Shader.PropertyToID("_RampInterpolations");

    private const int MaxPoints = 8;

    private Image image;
    private Material instanceMaterial;
    private ColorRamp cachedRamp;

    [SerializeField]
    private Shader shader;

    protected override void Awake()
    {
        base.Awake();
        image = GetComponent<Image>();
        EnsureMaterialInstance();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (image != null) image.SetMaterialDirty();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (image != null) image.SetMaterialDirty();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (instanceMaterial != null)
        {
            if (Application.isPlaying) Destroy(instanceMaterial);
            else DestroyImmediate(instanceMaterial);
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        if (image != null)
        {
            if (cachedRamp != null) ApplyRampToImage(cachedRamp);
            image.SetVerticesDirty();
            image.SetMaterialDirty();
        }
    }

    public Material GetModifiedMaterial(Material baseMaterial)
    {
        EnsureMaterialInstance();
        return instanceMaterial != null ? instanceMaterial : baseMaterial;
    }

    public void ApplyRampToImage(ColorRamp ramp)
    {
        if (ramp == null || ramp.points == null) return;
        cachedRamp = ramp;

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

        if (image != null) image.SetMaterialDirty();
    }

    private void EnsureMaterialInstance()
    {
        if (instanceMaterial == null && shader != null)
        {
            instanceMaterial = new Material(shader)
            {
                name = $"{gameObject.name}_ColorRamp_Inst",
                hideFlags = HideFlags.HideAndDontSave
            };
        }
    }
}