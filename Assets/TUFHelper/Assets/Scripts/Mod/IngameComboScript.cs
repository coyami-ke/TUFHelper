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

    public override bool IsShownOnlyInTUFHelper => false;
    public override string NameInSettings => "Combo";
    public override string ID => "Combo";
    public override Sprite Icon => Main.assets.LoadAsset<Sprite>("assets/tufhelper/assets/sprites/number.png");

    protected override void Awake()
    {
        base.Awake();
        if (staticText != null) staticTextRect = staticText.GetComponent<RectTransform>();
        if (comboText != null) comboTextRect = comboText.GetComponent<RectTransform>();
    }

    public override void OnSettingsOpened()
    {
    }

    protected override void OnPlay(PlayButtonEventArgs e)
    {
        _currentCombo = 0;
        UpdateComboUI();
    }

    protected override void OnHit(HitMargin hit)
    {
        if (IsActiveHit(hit))
        {
            _currentCombo++;
            UpdateComboUI();
        }
        else if (IsBreakingHit(hit))
        {
            _currentCombo = 0;
            UpdateComboUI();
        }

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

    private bool IsActiveHit(HitMargin hit)
    {
        switch (hit)
        {
            case HitMargin.Perfect:
            case HitMargin.EarlyPerfect:
            case HitMargin.LatePerfect:
            case HitMargin.VeryEarly:
            case HitMargin.VeryLate:
                return true;
            default:
                return false;
        }
    }

    private bool IsBreakingHit(HitMargin hit)
    {
        switch (hit)
        {
            case HitMargin.TooEarly:
            case HitMargin.TooLate:
            case HitMargin.Multipress:
            case HitMargin.FailMiss:
            case HitMargin.FailOverload:
            case HitMargin.OverPress:
                return true;
            default:
                return false;
        }
    }

    private void UpdateComboUI()
    {
        if (comboText != null)
        {
            comboText.text = _currentCombo.ToString();
        }
    }
}