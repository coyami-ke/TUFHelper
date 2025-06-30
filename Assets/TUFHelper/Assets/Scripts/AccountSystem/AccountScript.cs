using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using TUFHelper;
using TUFHelper.AccountSystem;
using UnityEngine;

public class AccountScript : MonoBehaviour
{
    private readonly TUFTokenRequest request = new();

    public TMP_InputField email, password;

    public GameObject background, window;

    public TextMeshProUGUI nicknameText, tagText;

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
            await GetToken(); // Await the task so exceptions are catchable
        }
        catch (Exception ex)
        {
            Main.Logger.Error("Login exception: " + ex.Message);
        }
    }

    private async Task GetToken()
    {
        await request.TryGetToken(email.text, password.text);
        AccountInfo = await request.GetInfoAboutMe();
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
