using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

public class AccountSettings : MonoBehaviour
{
    public Toggle ratingModeToggle;

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
        if (instance == null) instance = this;

        UpdateSettings();

        IsShow = false;
    }
    public void Start()
    {
        var account = AccountSaver.GetAccount();
        if (account == null) return;

        ratingModeToggle.isOn = account.IsRatingMode;
    }
    public void UpdateSettings()
    {
        ratingModeObject.SetActive(AccountScript.instance.IsSignedIn && (AccountScript.instance.AccountInfo.User.IsRater || AccountScript.instance.AccountInfo.User.IsSuperAdmin));
    }

    public void OnRatingModeChanged(bool value)
    {
        AccountScript.instance.AccountSaver.IsRatingMode = value;
        AccountScript.instance.AccountSaver.Save();

        Main.Logger.Log("Rating Mode : " + AccountScript.instance.AccountSaver.IsRatingMode);
    }
    public void ShowOrHideWindow()
    {
        IsShow = !IsShow;
    }
}
