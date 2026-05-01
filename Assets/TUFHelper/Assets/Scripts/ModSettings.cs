using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ModSettings : MonoBehaviour
{
    public GameObject window;

    public TextMeshProUGUI titleTab;

    public SettingTabPrefabScript[] tabs;
    private SettingTabPrefabScript _currentTab;
    public SettingTabPrefabScript CurrentTab
    {
        get => _currentTab;
        set
        {
            _currentTab = value;
            titleTab.text = _currentTab.nameTab;
            _currentTab.settingsObject.SetActive(true);
        }
    }

    public static ModSettings instance;

    public void Awake()
    {
        instance = this;
    }

    private bool _isShow;
    public bool IsShow
    {
        get => _isShow;
        set
        {
            _isShow = value;
            window.SetActive(value);
        }
    }

    public void ShowOrHideWindow()
    {
        IsShow = !IsShow;
    }
}
