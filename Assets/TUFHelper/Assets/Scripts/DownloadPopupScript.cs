using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DownloadPopupScript : MonoBehaviour
{
    public static DownloadPopupScript instance;

    
    public Slider Progress;
    public TextMeshProUGUI StateMessage;

    public static float ChangeProgress;
    public static string ChangeMessage;
    
    public void Awake()
    {
        instance = this;

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
        if (instance == null)
            instance = GameObject.Find("Canvas").transform.Find("DownloadPopup").GetComponent<DownloadPopupScript>();
        
        ChangeMessage = null;
        ChangeProgress = 0;

        
        instance.gameObject.SetActive(false);
    }
    
    public static void Show()
    {
        if (instance == null)
            instance = GameObject.Find("Canvas").transform.Find("DownloadPopup").GetComponent<DownloadPopupScript>();
        

        ChangeMessage = null;
        ChangeProgress = 0;

        instance.StateMessage.text = "";
        instance.Progress.value = 0;
        instance.gameObject.SetActive(true);
    }
}
