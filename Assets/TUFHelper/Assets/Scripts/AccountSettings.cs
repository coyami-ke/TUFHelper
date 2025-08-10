using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Web;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

public class AccountSettings : MonoBehaviour
{
    public Toggle ratingModeToggle;

    public Toggle showOnlyDownloadedToggle;
    public TMP_InputField searchField;

    private bool _isShow = false;
    public bool IsShow
    {
        get => _isShow;
        set
        {
            _isShow = value;

            if (!value) windowRect.DOSizeDelta(new(windowRect.sizeDelta.x, 0), 0.5f).SetEase(Ease.OutExpo);
            else windowRect.DOSizeDelta(new(windowRect.sizeDelta.x, HeightWindow), 0.5f).SetEase(Ease.OutExpo);
            backgroundObject.SetActive(value);
        }
    }

    public float HeightWindow;

    public bool IsRatingMode { get; private set; }

    public GameObject ratingModeObject, windowObject, backgroundObject;
    public RectTransform windowRect;

    public static AccountSettings instance { get; private set; }
    public void Awake()
    {
        instance = this;
    }

    public void StartWindowsManager()
    {
        UpdateSettings();

        var account = AccountSaver.GetAccount();
        if (account == null) return;

        ratingModeToggle.isOn = ADOFAIGameplayHandler.EditorPlayPatch.RatingMode;

        OnRatingModeChanged(ratingModeToggle.isOn);

        Main.Logger.Log(ratingModeToggle.isOn.ToString());

        IsShow = false;
    }
    public void UpdateSettings()
    {
        ratingModeObject.SetActive(AccountScript.instance.IsSignedIn && (AccountScript.instance.AccountInfo.User.IsRater || AccountScript.instance.AccountInfo.User.IsSuperAdmin));
    }


    public void OnRatingModeChanged(bool value)
    {
        AccountScript.instance.AccountSaver.IsRatingMode = value;
        AccountScript.instance.AccountSaver.Save();

        if (value) WindowsManager.instance.ShowRatingPanel();
        else
        {
            WindowsManager.instance.HideRatingPanel();
            ADOFAIGameplayHandler.EditorPlayPatch.RatingMode = false;
        }
        // if (value)
            // {
            //     searchField.text = "#";
            //     showOnlyDownloadedToggle.isOn = false;
            //     LevelListScript.instance.ClearLevels();
            // }

            RatingPanel.instance?.UpdateList();

        if (value) CustomMusicPlayer.instance.StopPlay();
    }
    public void ShowOrHideWindow()
    {
        IsShow = !IsShow;
    }
}
