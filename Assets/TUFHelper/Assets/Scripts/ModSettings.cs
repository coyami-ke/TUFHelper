using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModSettings : MonoBehaviour
{
    public GameObject backgroundImage, window;

    public SettingTabPrefabScript[] tabs;

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
            backgroundImage.SetActive(value);
            window.SetActive(value);
        }
    }

    public void ShowOrHideWindow()
    {
        IsShow = !IsShow;
    }
}
