using System;
using TMPro;
using TUFHelper;
using UnityEngine;

public class LanguageSettings : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    private static readonly string[] LanguageModes = { "Auto", "English", "Korean", "Chinese" };

    public void Start()
    {
        string currentLanguage = Main.Setting?.Language ?? "Auto";

        int targetIndex = Array.IndexOf(LanguageModes, currentLanguage);
        dropdown.value = targetIndex >= 0 ? targetIndex : 0;
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        dropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    public void OnLanguageChanged(int index)
    {
        if (index < 0 || index >= LanguageModes.Length) return;

        string selectedMode = LanguageModes[index];

        Main.Setting.Language = selectedMode;

        LanguageManager.SetLanguageMode(selectedMode);

        LanguageManager.ApplyTo(ModSettings.instance.gameObject);
    }
}