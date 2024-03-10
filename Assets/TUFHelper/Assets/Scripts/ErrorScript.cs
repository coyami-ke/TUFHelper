using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorScript : MonoBehaviour
{
    public static ErrorScript instance;

    public TextMeshProUGUI errorContentText;

    public void Awake()
    {
        instance = this;

        gameObject.SetActive(false);
    }
    

    public void CloseButtonClick()
    {
        gameObject.SetActive(false);
    }

    public static void ShowError(string message)
    {
        DownloadPopupScript.Close();
        instance.errorContentText.text = message;
        instance.gameObject.SetActive(true);
    }
}
