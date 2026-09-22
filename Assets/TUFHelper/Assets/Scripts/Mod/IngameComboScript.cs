using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DG.Tweening;
using TMPro;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;

[RegisterIngameElement("Combo", "assets/tufhelper/assets/prefabs/ingamecombo.prefab")]
public class IngameComboScript : BasicIngameElement
{
    public TextMeshProUGUI staticText, comboText;

    private int _currentCombo = 0;
    private RectTransform staticTextRect;
    private RectTransform comboTextRect;

    private ComboSettingsCategory comboSettings;

    public override bool IsShownOnlyInTUFHelper => false;
    public override string NameInSettings => "Combo";
    public override string ID => "Combo";
    public override Sprite Icon => Main.assets.LoadAsset<Sprite>("assets/tufhelper/assets/sprites/number.png");
    public override Anchor DefaultAnchor => Anchor.MiddleTop;
    public override Vector2 DefaultPosition => new(0, -125);
    public override TextMeshProUGUI[] Texts => new TextMeshProUGUI[2] { staticText, comboText };

    protected override void Awake()
    {
        base.Awake();
        if (staticText != null) staticTextRect = staticText.GetComponent<RectTransform>();
        if (comboText != null) comboTextRect = comboText.GetComponent<RectTransform>();
    }
    protected override void Start()
    {
        _currentCombo = 0;
        base.Start();
        UpdateComboUI();
    }

    public override void OnSettingsOpened()
    {
        _currentCombo = 0;
        UpdateComboUI();
        UpdateStaticText();
    }
    protected override void OnPlay(PlayButtonEventArgs e)
    {
        base.OnPlay(e);
        _currentCombo = 0;
        UpdateComboUI();
        UpdateStaticText();
    }

    protected override void OnLoadCustomSettings(IngameElementModel model)
    {
        if (comboSettings != null)
        {
            comboSettings.PropertyChanged -= ComboSettings_PropertyChanged;
        }

        comboSettings = model.GetCategory("Combo", new ComboSettingsCategory());
        comboSettings.PropertyChanged += ComboSettings_PropertyChanged;

        UpdateStaticText();
    }
    private void ComboSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ComboSettingsCategory.UseXPerfectSystem) ||
            e.PropertyName == nameof(ComboSettingsCategory.CustomText))
        {
            UpdateStaticText();
        }
    }

    private void UpdateStaticText()
    {
        if (staticText == null) return;

        if (!string.IsNullOrEmpty(comboSettings?.CustomText))
        {
            staticText.text = comboSettings.CustomText;
            return;
        }

        bool useXPerfect = comboSettings?.UseXPerfectSystem ?? true;
        staticText.text = useXPerfect ? "X-Perfect" : "Perfect";
    }

    private void AnimateText()
    {
        if (staticTextRect != null)
        {
            staticTextRect.DOKill();
            staticTextRect.localScale = new Vector3(1.33f, 1.2f, 1f);
            staticTextRect.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutCirc);
        }

        if (comboTextRect != null)
        {
            comboTextRect.DOKill();
            comboTextRect.localScale = new Vector3(1.5f, 1.33f, 1f);
            comboTextRect.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutCirc);
        }
    }

    private void UpdateComboUI()
    {
        comboText.text = _currentCombo.ToString();
    }

    protected override void OnHit(HitMargin hit)
    {
        if (hit == HitMargin.Auto || hit == HitMargin.Midspin) return;

        bool useXPerfect = comboSettings?.UseXPerfectSystem ?? true;

        if (useXPerfect)
        {
            if (hit == HitMargin.XPerfect)
            {
                _currentCombo++;
            }
            else
            {
                _currentCombo = 0;
            }
        }
        else
        {
            if (IsStandardPerfectHit(hit))
            {
                _currentCombo++;
            }
            else if (IsComboBreakHit(hit))
            {
                _currentCombo = 0;
            }
        }

        UpdateComboUI();
        AnimateText();
    }
    private bool IsStandardPerfectHit(HitMargin hit)
    {
        switch (hit)
        {
            case HitMargin.XPerfect:
            case HitMargin.PerfectPlus:
            case HitMargin.PerfectMinus:
            case HitMargin.EarlyPerfect:
            case HitMargin.LatePerfect:
            case HitMargin.VeryEarly:
            case HitMargin.VeryLate:
                return true;
            default:
                return false;
        }
    }
    private bool IsComboBreakHit(HitMargin hit)
    {
        switch (hit)
        {
            case HitMargin.TooEarly:
            case HitMargin.TooLate:
            case HitMargin.FailMiss:
            case HitMargin.FailOverload:
            case HitMargin.Multipress:
            case HitMargin.OverPress:
                return true;
            default:
                return false;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (comboSettings != null)
        {
            comboSettings.PropertyChanged -= ComboSettings_PropertyChanged;
        }
    }
}

public partial class ComboSettingsCategory : IngameElementSettingsCategory
{
    [ObservableProperty]
    [property: ShowInOverlayerSettings("Use XPerfect")]
    private bool _useXPerfectSystem = true;

    [ObservableProperty]
    [property: ShowInOverlayerSettings("Custom Text")]
    private string _customText = "";

    public override string DisplayName => "Combo";

    public override Sprite Icon => Main.assets.LoadAsset<Sprite>("assets/tufhelper/assets/sprites/number.png");
}