using CommunityToolkit.Mvvm.ComponentModel;
using TMPro;
using TUFHelper;
using UnityEngine;
public abstract class IngameVariableScript : BasicIngameElement
{
    protected abstract TextMeshProUGUI GetText { get; }
    protected IngameVariableSettingsCategory VariableSettings { get; private set; }
    protected abstract string DefaultVariableName { get; }

    protected override void Start()
    {
        base.Start();
        UpdateText(""); // Safe now because UpdateText handles null VariableSettings
    }

    protected override void OnLoadCustomSettings(IngameElementModel model)
    {
        VariableSettings = model.GetCategory<IngameVariableSettingsCategory>(
            "Variable",
            new() { VariableName = DefaultVariableName }
        );

        model.PropertyChanged += Model_PropertyChanged;

        UpdateText("");
    }

    private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        UpdateText("");
    }

    protected void UpdateText(string value)
    {
        if (GetText == null) return;

        string varName = VariableSettings?.VariableName ?? DefaultVariableName;
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
    public override string DisplayName => "Variable";
    public override Sprite Icon => null;
}
