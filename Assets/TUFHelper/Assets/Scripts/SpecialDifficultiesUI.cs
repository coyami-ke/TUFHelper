using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TUFHelper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpecialDifficultiesUI : MonoBehaviour, IPointerClickHandler
{
    public static SpecialDifficultiesUI instance;

    public Image arrowImage, backgroundImage;
    public RectTransform menuTransform;

    public Toggle marathonToggle, gimmickToggle, unrankedToggle, minus2Toggle, minus21Toggle;
    public Toggle qqToggle, q2Toggle, q2PlusToggle, q3Toggle, q3PlusToggle, q4Toggle;

    private readonly Color selectedColor = new(1f, 1f, 1f, 20f / 255f);
    private readonly Color unselectedColor = new(1f, 1f, 1f, 10f / 255f);

    private const float menu_MaxPositionY = -205;
    private const float menu_MinPositionY = -30;
    private const float selectedRotation = -90;
    private const float unselectedRotation = 90;

    private Dictionary<string, Toggle> toggleMap;

    private bool _isSelected = false;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            AnimateDropdown(value);
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        toggleMap = new Dictionary<string, Toggle>
        {
            { "Marathon", marathonToggle },
            { "Gimmick", gimmickToggle },
            { "Unranked", unrankedToggle },
            { "-2", minus2Toggle },
            { "-21", minus21Toggle },
            { "Qq", qqToggle },
            { "Q2", q2Toggle },
            { "Q2+", q2PlusToggle },
            { "Q3", q3Toggle },
            { "Q3+", q3PlusToggle },
            { "Q4", q4Toggle },
        };

        foreach (var kvp in toggleMap)
        {
            string key = kvp.Key;
            Toggle toggle = kvp.Value;

            toggle.isOn = Main.Setting.SelectedSpecialDiffs.Contains(key);
            toggle.onValueChanged.AddListener(_ => UpdateLevelList());
        }

        IsSelected = false;

        UpdateLevelList();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        IsSelected = !IsSelected;
    }

    private void AnimateDropdown(bool show)
    {
        backgroundImage.DOColor(show ? selectedColor : unselectedColor, 0.4f).SetEase(Ease.OutExpo);
        arrowImage.rectTransform.DORotate(new Vector3(0f, 0f, show ? selectedRotation : unselectedRotation), 0.4f).SetEase(Ease.OutExpo);
        menuTransform.DOAnchorPos3DY(show ? menu_MaxPositionY : menu_MinPositionY, 0.4f).SetEase(Ease.OutExpo);
        menuTransform.DOScaleY(show ? 1f : 0f, 0.4f).SetEase(Ease.OutExpo);
    }

    public void UpdateLevelList()
    {
        var selectedDiffs = toggleMap
            .Where(kvp => kvp.Value.isOn)
            .Select(kvp => kvp.Key)
            .ToList();

        Main.Setting.SelectedSpecialDiffs = selectedDiffs;
        Main.Setting.Save(Main.ModEntry);

        LevelListScript.DefaultRequest.SpecialDifficulties = selectedDiffs;
        LevelListScript.instance.ClearLevels();
        LevelListScript.instance.UpdateLevelList();
    }
}
