using CommunityToolkit.Mvvm.ComponentModel;
using TMPro;
using TUFHelper;
using UnityEngine;

public abstract class IngameVariableScript : BasicIngameElement
{
    protected abstract TextMeshProUGUI GetText { get; }
    protected IngameVariableSettingsCategory VariableSettings { get; private set; }
    protected abstract string DefaultVariableName { get; }
    protected abstract float MaxValue { get; }
    protected abstract float MinValue { get; }

    protected override void Start()
    {
        base.Start();
        UpdateText("");
    }

    protected override void OnLoadCustomSettings(IngameElementModel model)
    {
        VariableSettings = model.GetCategory<IngameVariableSettingsCategory>(
            "Variable",
            new()
            {
                VariableName = DefaultVariableName,
                AdjustColorByValueGradient = new(new ColorRamp.RampPoint[5]
                {
                    new() { position = 0, ColorRgba = new float[4] { 0.33f, 0.32f, 1.0f, 1.0f }, interpolation = ColorRamp.InterpolationType.CatmullRom },
                    new() { position = 0.25f, ColorRgba = new float[4] { 0.47f, 0.75f, 1.0f, 1.0f }, interpolation = ColorRamp.InterpolationType.CatmullRom },
                    new() { position = 0.5f, ColorRgba = new float[4] { 0.62f, 0.29f, 1.0f, 1.0f }, interpolation = ColorRamp.InterpolationType.CatmullRom },
                    new() { position = 0.75f, ColorRgba = new float[4] { 0.76f, 0.37f, 0.9f, 1.0f }, interpolation = ColorRamp.InterpolationType.CatmullRom },
                    new() { position = 1.0f, ColorRgba = new float[4] { 1.0f, 0.16f, 0.16f, 1.0f }, interpolation = ColorRamp.InterpolationType.CatmullRom }
                })
            }
        );

        model.PropertyChanged += Model_PropertyChanged;

        UpdateText("");
    }

    private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        UpdateText("");
    }

    protected void UpdateText(float value, string format = "F0")
    {
        if (GetText == null) return;

        float range = MaxValue - MinValue;
        if (Mathf.Approximately(range, 0f))
        {
            range = 0.0001f;
        }

        float rawPos = (value - MinValue) / range;
        float normalizedPos = Mathf.Clamp01(rawPos);

        Color color = Color.white;
        if (VariableSettings?.AdjustColorByValueGradient != null)
        {
            color = VariableSettings.AdjustColorByValueGradient.Evaluate(normalizedPos);

            if (color == default || color.a <= 0f)
            {
                color = Color.white;
            }
        }

        string valueText = value.ToString(format);
        string coloredValue = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{valueText}</color>";

        string varName = VariableSettings?.VariableName ?? DefaultVariableName;
        GetText.text = $"{varName}: {coloredValue}";
    }

    protected void UpdateText(string value)
    {
        if (GetText == null) return;

        string varName = VariableSettings?.VariableName ?? DefaultVariableName;

        if (string.IsNullOrEmpty(value))
        {
            GetText.text = $"{varName}: ";
            return;
        }

        GetText.text = $"{varName}: {value}";
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (Model != null)
        {
            Model.PropertyChanged -= Model_PropertyChanged;
        }
    }
}

public partial class IngameVariableSettingsCategory : IngameElementSettingsCategory
{
    [ObservableProperty]
    [property: ShowInOverlayerSettings("Name")]
    private string variableName;

    [ObservableProperty]
    [property: ShowInOverlayerSettings("Adjust Color By Value")]
    private ColorRamp adjustColorByValueGradient = new();

    public override string DisplayName => "Variable";
    public override Sprite Icon => Main.assets.LoadAsset<Sprite>("assets/tufhelper/assets/sprites/variable.png");
}
