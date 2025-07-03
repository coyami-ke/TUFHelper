using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using TUFHelper;
using TUFHelper.AccountSystem;
using UnityEngine;
using UnityEngine.Networking;

public class AccountScript : MonoBehaviour
{
    private readonly TUFTokenRequest request = new();

    public TMP_InputField email, password;

    public GameObject background, window;

    public TextMeshProUGUI nicknameText, tagText, errorMessage;

    public static AccountScript instance { get; private set; }

    public FullInfoAboutMyAccount AccountInfo { get; private set; }

    public void Awake()
    {
        if (instance == null) instance = this;
    }
    public async void EnterButton()
    {
        try
        {
            await GetToken();
        }
        catch (Exception ex)
        {
            Main.Logger.Error("Login exception: " + ex.Message);
        }
    }

    public void HideWindow()
    {
        background.SetActive(false);
        window.SetActive(false);
    }

    private async Task GetToken()
    {
        await request.TryGetToken(email.text, password.text);

        switch (request.LastResponseCode)
        {
            case 401:
                errorMessage.text = "Wrong Email/Username or Password";
                return;
            case 400:
                errorMessage.text = "Wrong formal";
                return;
            case 404:
                errorMessage.text = "Hmmn, the site crashed. Please try again later. Code: 404";
                return;
            case 200:
                AccountInfo = await request.GetInfoAboutMe();
                return;
        }
    }


    public void SignInButtonClick()
    {
        nicknameText.text = "";
        tagText.text = "";
        email.text = "";
        password.text = "";

        background.SetActive(true);
        window.SetActive(true);
    }
}
