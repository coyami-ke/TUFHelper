using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TUFHelper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CurationTypesScript : MonoBehaviour, IPointerClickHandler
{
    public Image arrowImage, backgroundImage;
    public RectTransform menuTransform;

    public Toggle c3, c2, c1, c0, o3, o2, o1, v3, v2, v1, v0, h2, h1, epic;

    public float maxHeight;

    private readonly Color selectedColor = new(165f / 255, 183f / 255, 255f / 255, 30f / 255f);
    private readonly Color unselectedColor = new(165f / 255, 183f / 255, 255f / 255, 20f / 255f);
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

    private async void Start()
    {
        menuTransform.sizeDelta = new(menuTransform.sizeDelta.x, 0);
        toggleMap = new Dictionary<string, Toggle>
        {
            { "C3", c3 },
            { "C2", c3 },
            { "C1", c3 },
            { "C0", c3 },
            { "O3", o3 },
            { "O2", o2 },
            { "O1", o1 },
            { "V3", v3 },
            { "V2", v2 },
            { "V1", v1 },
            { "V0", v0 },
            { "H2", h2 },
            { "H1", h1 },
            { "Epic", epic },
        };

        foreach (var kvp in toggleMap)
        {
            string key = kvp.Key;
            Toggle toggle = kvp.Value;

            toggle.isOn = Main.Setting.SelectedSpecialDiffs.Contains(key);
            toggle.onValueChanged.AddListener(async _ => await UpdateLevelList());
        }

        IsSelected = false;

        await UpdateLevelList();
    }

    private void AnimateDropdown(bool show)
    {
        backgroundImage.DOColor(show ? selectedColor : unselectedColor, 0.4f).SetEase(Ease.OutExpo);
        arrowImage.rectTransform.DORotate(new Vector3(0f, 0f, show ? selectedRotation : unselectedRotation), 0.4f).SetEase(Ease.OutExpo);
        menuTransform.DOSizeDelta(new(menuTransform.sizeDelta.x, show ? maxHeight : 0), 0.4f).SetEase(Ease.OutExpo);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        IsSelected = !IsSelected;
    }

    public async Task UpdateLevelList()
    {
        var selectedDiffs = toggleMap
            .Where(kvp => kvp.Value.isOn)
            .Select(kvp => kvp.Key)
            .ToList();

        Main.Setting.SelectedCurationTypes = selectedDiffs;
        Main.Setting.Save(Main.ModEntry);

        LevelListScript.DefaultRequest.CurationTypes = selectedDiffs;
        LevelListScript.instance.ClearLevels();
        await LevelListScript.instance.UpdateLevelListAsync();
    }
}
