using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DownloadPopupScript : MonoBehaviour
{
    public static DownloadPopupScript instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.Find("Canvas").transform.Find("DownloadPopup").GetComponent<DownloadPopupScript>();
            
            if (_instance == null)
                _instance = FindObjectOfType<DownloadPopupScript>();
            
            return _instance;
        }
    }

    private static DownloadPopupScript _instance;
    public static bool IsDownloading;
    public static float ChangeProgress;
    public static string ChangeMessage;

    public TextMeshProUGUI WarningText;
    public Button ContinueDownloadButton;
    public Button CancelButton;
    
    public Slider Progress;
    public TextMeshProUGUI StateMessage;
    
    
    public void Awake()
    {
        if (instance == null)
        {
            _instance = this;
            gameObject.SetActive(false);
        }
    }


    public static void ShowFileWarning(long fileSize, Action continueDownload, Action cancelDownload)
    {
        Close();
        
        instance.WarningText.transform.parent.gameObject.SetActive(true);
        instance.WarningText.text = $"This file is quite large at {DirectLevel.Utils.ByteToStringUnit(fileSize)}, are you sure you want to download it?";
        
        instance.ContinueDownloadButton.onClick.RemoveAllListeners();
        instance.CancelButton.onClick.RemoveAllListeners();
        
        instance.ContinueDownloadButton.onClick.AddListener(()=>scrSfx.instance?.PlaySfx(SfxSound.MobileButton));
        instance.CancelButton.onClick.AddListener(()=>scrSfx.instance?.PlaySfx(SfxSound.MobileButton));

        instance.ContinueDownloadButton.onClick.AddListener(() =>
        {
            Show();
            
            continueDownload();
            instance.WarningText.transform.parent.gameObject.SetActive(false);
        });
        instance.CancelButton.onClick.AddListener(() =>
        {
            cancelDownload();
            instance.WarningText.transform.parent.gameObject.SetActive(false);
        });
    }

    private void Update()
    {
        if (ChangeProgress != 0)
        {
            Progress.value = ChangeProgress;
            ChangeProgress = 0;
        }
        
        if (ChangeMessage != null)
        {
            StateMessage.text = ChangeMessage;
            ChangeMessage = null;
        }
    }

    public static void Close()
    {
        
        ChangeMessage = null;
        ChangeProgress = 0;

        
        instance.gameObject.SetActive(false);
    }
    
    public static void Show()
    {

        ChangeMessage = null;
        ChangeProgress = 0;

        instance.StateMessage.text = "";
        instance.Progress.value = 0;
        instance.gameObject.SetActive(true);
    }
}
