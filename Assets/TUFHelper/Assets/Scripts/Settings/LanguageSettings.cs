using System;
using TMPro;
using TUFHelper;
using UnityEngine;

public class LanguageSettings : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    private static readonly string[] LanguageModes = { "Auto", "English", "Korean", "Chinese", "SpanishLatinAmerica" };

    public void Start()
    {
        if (dropdown.options.Count < LanguageModes.Length)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData("Spanish (Latin America)"));
        }

        string currentLanguage = Main.Setting?.Language ?? "Auto";

        int targetIndex = Array.IndexOf(LanguageModes, currentLanguage);
        dropdown.SetValueWithoutNotify(targetIndex >= 0 ? targetIndex : 0);
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        dropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    public void OnLanguageChanged(int index)
    {
        if (index < 0 || index >= LanguageModes.Length) return;

        string selectedMode = LanguageModes[index];

        LanguageManager.SetLanguageMode(selectedMode);
    }
}
