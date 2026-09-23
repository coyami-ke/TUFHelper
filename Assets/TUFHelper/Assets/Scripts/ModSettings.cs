using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
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
            UITransition.Show(_currentTab.settingsObject, 0.2f);
        }
    }

    public static ModSettings instance;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        LanguageManager.ApplyTo(gameObject);
    }

    public void OnEnable()
    {
        LanguageManager.ApplyTo(gameObject);
    }

    private bool _isShow;
    public bool IsShow
    {
        get => _isShow;
        set
        {
            _isShow = value;
            UITransition.SetVisible(window, value);
        }
    }

    public void ShowOrHideWindow()
    {
        IsShow = !IsShow;
    }
}
