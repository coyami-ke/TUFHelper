using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

public class SettingsScript : MonoBehaviour
{

    public static SettingsScript instance;
    public static int cachedMinDiffDropdownValue = 0, cachedMaxDiffDropdownValue = 0;

    public Toggle legacyDiffToggle, showUnratedLevelToggle;
    public Image logoImage;
    public TMP_Dropdown minDiffDropdown, maxDiffDropdown;

    public Sprite tufLogoSprite, t21cLogoSprite;

    public void Awake()
    {
        if (Main.Setting == null)
        {
            Main.Setting = new Setting();
        }

        instance = this;
        legacyDiffToggle.isOn = Main.Setting.showLegacyRating;
        showUnratedLevelToggle.isOn = Main.Setting.showUnratedLevels;
        logoImage.sprite = Main.Setting.showLegacyRating ? t21cLogoSprite : tufLogoSprite;

        StartCoroutine(UpdateDifficultyDropdownsCo());
    }
    
    public IEnumerator UpdateDifficultyDropdownsCo()
    {
        List<string> categories = new List<string>() { "P", "G", "U" };
        List<OptionData> options = new List<OptionData>();
        foreach (string category in categories)
        {
            for (int i = 1; i <= 20; i++)
            {
                if (category.Equals("U"))
                {
                    if (Main.Setting.showLegacyRating)
                    {
                        i++; // skip one
                    }
                    if (i > 14)
                    {
                        continue;
                    }
                }
                string pgu = category + i;
                options.Add(new OptionData(pgu, Helper.getDiffSprite(pgu)));
            }
        }
        options.Add(new OptionData("-21", Helper.getDiffSprite("-21")));

        minDiffDropdown.ClearOptions();
        minDiffDropdown.AddOptions(options);

        options.Reverse();
        maxDiffDropdown.ClearOptions();
        maxDiffDropdown.AddOptions(options);

        int minDiffVal = cachedMinDiffDropdownValue, maxDiffVal = cachedMaxDiffDropdownValue;
        if (minDiffDropdown.value != minDiffVal)
        {
            minDiffDropdown.value = minDiffVal;
        }

        if (maxDiffDropdown.value != maxDiffVal)
        {
            maxDiffDropdown.value = maxDiffVal;
        }

        yield return null;
    }

    public void OnDifficultyDropdownChange()
    {
        cachedMinDiffDropdownValue = minDiffDropdown.value;
        cachedMaxDiffDropdownValue = maxDiffDropdown.value;

        PageSwitcherScript.cachedPage = 1;
        if (LevelListScript.instance != null)
        {
            StartCoroutine(LevelListScript.instance.LoadLevelListCo());
        }
    }

    public void ToggleLegacyRating()
    {
        if (minDiffDropdown.options.Count > 10) // check is dropdown initialized
        {
            cachedMinDiffDropdownValue = 0;
            cachedMaxDiffDropdownValue = 0;
        }

        Main.Setting.showLegacyRating = legacyDiffToggle.isOn;
        logoImage.sprite = Main.Setting.showLegacyRating ? t21cLogoSprite : tufLogoSprite;

        Main.Setting.Save(Main.ModEntry);

        if (LevelListScript.instance != null)
        {
            StartCoroutine(LevelListScript.instance.LoadLevelListCo());
        }
        StartCoroutine(UpdateDifficultyDropdownsCo());
    }

    public void ToggleUnratedLevels()
    {
        Main.Setting.showUnratedLevels = showUnratedLevelToggle.isOn;
        if (LevelListScript.instance != null)
        {
            StartCoroutine(LevelListScript.instance.LoadLevelListCo());
        }
    }
}
