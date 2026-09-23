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
        UITransition.Hide(gameObject, 0.16f);
    }

    public static void ShowError(string message)
    {
        if (message.StartsWith("The request was")) return;
        //DownloadPopupScript.Close();
        instance.errorContentText.text = message;
        UITransition.Show(instance.gameObject, 0.2f);
    }
}
