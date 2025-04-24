using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

public class SettingsScript : MonoBehaviour
{

    public static SettingsScript instance;

    public Toggle showUnratedLevelToggle, showExtraLevelToggle, showHiddenLevelToggle;
    public Image logoImage;
    public TMP_Dropdown minDiffDropdown, maxDiffDropdown;

    public void Awake()
    {
        if (Main.Setting == null)
        {
            Main.Setting = new Setting();
        }

        instance = this;

        StartCoroutine(UpdateDifficultyDropdownsCo());
    }
    
    public IEnumerator UpdateDifficultyDropdownsCo()
    {
        List<OptionData> options = new();
        foreach (var pair in DiffSpriteHelper.DiffIDRegister)
        {
            int id = pair.Key;
            string diff = pair.Value;

            if (DiffSpriteHelper.IsSpecialDiff(diff)) continue;
            Sprite sprite = Main.assets.LoadAsset<Sprite>(DiffSpriteHelper.GetSpriteFromId(id));
            options.Add(new OptionData(diff, sprite));
        }
        minDiffDropdown.ClearOptions();
        minDiffDropdown.AddOptions(options);

        maxDiffDropdown.ClearOptions();
        maxDiffDropdown.AddOptions(options);
        maxDiffDropdown.value = options.Count - 1;
        maxDiffDropdown.RefreshShownValue();

        yield return null;
    }

    public void OnDifficultyDropdownChange()
    {
        if (LevelListScript.instance != null)
        {
            LevelListScript.instance.UpdateLevelList(SearchScript.instance.searchField.text);
        }
    }

    public void ToggleUnratedLevels()
    {
        Main.Setting.showUnratedLevels = showUnratedLevelToggle.isOn;
        if (LevelListScript.instance != null)
        {
            LevelListScript.instance.UpdateLevelList(SearchScript.instance.searchField.text);
        }
    }
    public void ToggleExtraLevels()
    {
        if (LevelListScript.instance != null)
        {
            LevelListScript.instance.UpdateLevelList(SearchScript.instance.searchField.text);
        }
    }
    public void ToggleHiddenLevels()
    {
        if (LevelListScript.instance != null)
        {
            LevelListScript.instance.UpdateLevelList(SearchScript.instance.searchField.text);
        }
    }
}
