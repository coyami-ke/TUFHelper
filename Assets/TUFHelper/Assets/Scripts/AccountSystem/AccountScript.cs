using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using TUFHelper;
using TUFHelper.AccountSystem;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AccountScript : MonoBehaviour
{
    private TUFTokenRequest request = new();
    public AccountSaver AccountSaver { get; private set; } = new();
    public TUFTokenRequest TokenRequest { get => request; }

    public TMP_InputField email, password;

    public GameObject background, window, signInButton, logOutButton;
    public Image pfpImage;

    public TextMeshProUGUI nicknameText, tagText, errorMessage;

    public static AccountScript instance { get; private set; }

    public FullInfoAboutMyAccount AccountInfo { get; private set; }

    public bool IsSignedIn { get; private set; } = false;

    public void Awake()
    {
        if (instance == null) instance = this;
    }

    public async void LoadProfilePicture(string url)
    {
        byte[] imageData = await request.GetPfpFromURL(url);
        if (imageData == null) return;

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        pfpImage.sprite = sprite;
    }

    public async void Start()
    {
        if (File.Exists(AccountSaver.PATH_TO_ACCOUNT_FILE))
        {
            AccountSaver = AccountSaver.GetAccount();
            request.Token = AccountSaver.Token;

            AccountInfo = await request.GetInfoAboutMe();

            IsSignedIn = request.LastResponseCode == 200;
        }
        else
        {
            IsSignedIn = false;
        }

        UpdateAccountVisuals();
    }


    public void UpdateAccountVisuals()
    {
        if (IsSignedIn && AccountInfo?.User != null)
        {
            nicknameText.text = AccountInfo.User.Username;
            tagText.text = "@" + AccountInfo.User.Nickname;

            signInButton.SetActive(false);
            logOutButton.SetActive(true);
            pfpImage.gameObject.SetActive(true);

            LoadProfilePicture(AccountInfo.User.AvatarUrl);
        }
        else
        {
            nicknameText.text = "";
            tagText.text = "";
            signInButton.SetActive(true);
            logOutButton.SetActive(false);
            pfpImage.gameObject.SetActive(false);
        }

        AccountSettings.instance.UpdateSettings();
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
            errorMessage.text = "An error occurred during login.";
        }
    }

    public void RegisterAccount()
    {
        string url = "https://tuforums.com/register";
        Application.OpenURL(url);
    }

    public void LogOut()
    {
        if (File.Exists(AccountSaver.PATH_TO_ACCOUNT_FILE))
        {
            File.Delete(AccountSaver.PATH_TO_ACCOUNT_FILE);
        }

        IsSignedIn = false;
        request = new();
        AccountSaver = new();

        UpdateAccountVisuals();
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
                errorMessage.text = "Invalid format or input data.";
                return;
            case 404:
                errorMessage.text = "Hmmn, the site crashed. Please try again later. Code: 404";
                return;
            case 200:
                AccountInfo = await request.GetInfoAboutMe();
                AccountSaver.Token = request.Token;
                AccountSaver.Save();
                Main.Logger.Log("The token has been saved!");

                IsSignedIn = true;

                UpdateAccountVisuals();
                HideWindow();
                return;
            default:
                errorMessage.text = $"Unexpected error. Code: {request.LastResponseCode}";
                return;
        }
    }

    public void SignInButtonClick()
    {
        nicknameText.text = "";
        tagText.text = "";
        email.text = "";
        password.text = "";
        errorMessage.text = "";

        background.SetActive(true);
        window.SetActive(true);
    }
}
