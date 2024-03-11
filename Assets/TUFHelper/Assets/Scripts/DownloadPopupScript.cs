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
    
    public Slider Progress;
    public TextMeshProUGUI StateMessage;
    
    
    public void Awake()
    {
        _instance = this;

        gameObject.SetActive(false);
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
