using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rewired.UI.ControlMapper;
using TMPro;
using Together.Utils;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PassInfo : MonoBehaviour
{
    public static PassInfo instance { get; private set; }

    public TextMeshProUGUI perfectsText, lPerfectsText, ePerfectsText, latesText, earliesText, missesText;
    public TextMeshProUGUI levelNameText, compositorText, authorText, chartText, vfxText, scoreText, playerText;
    public TextMeshProUGUI newPlayerScoreText, oldPlayerScoreText, addPlayerScoreText, rankText;
    public TextMeshProUGUI accuracyText, speedText, feelingRating;
    public TextMeshProUGUI scorePPText;
    public RectTransform perfectsRect, lPerfectsRect, ePerfectsRect, latesRect, earliesRect, missesRect;

    public float maxHeight = 200;
    public float minPosX = 10;
    public Image levelIcon;
    private string passVideoUrl;
    private GameObject videoButton;
    public void Awake()
    {
        if (instance == null) instance = this;
    }
    public void SetHeight(RectTransform rectTransform, float newHeight)
    {
        Vector2 size = rectTransform.sizeDelta;
        size.y = newHeight;
        rectTransform.sizeDelta = size;

        Vector2 anchoredPosition = rectTransform.anchoredPosition;
        anchoredPosition.y = minPosX + newHeight / 2;
        rectTransform.anchoredPosition = anchoredPosition;
    }

    public void BackToMainMenu()
    {
        WindowsManager.instance.MoveToLevelList();
    }
    public void OpenPassVideoLink()
    {
        if (string.IsNullOrWhiteSpace(passVideoUrl)) return;
        Application.OpenURL(passVideoUrl);
    }

    private void SetVideoButton(string videoUrl)
    {
        bool valid = System.Uri.TryCreate(videoUrl, System.UriKind.Absolute, out System.Uri videoUri) &&
                     (videoUri.Scheme == System.Uri.UriSchemeHttps || videoUri.Scheme == System.Uri.UriSchemeHttp);

        passVideoUrl = valid ? videoUrl : null;

        if (videoButton == null && valid)
            CreateVideoButton();

        if (videoButton != null)
            videoButton.SetActive(valid);
    }

    private void CreateVideoButton()
    {
        Transform player = transform.Find("Player");
        if (player == null) return;

        Transform existing = player.Find("VideoButton");
        if (existing != null)
        {
            videoButton = existing.gameObject;
        }
        else
        {
            videoButton = new GameObject("VideoButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            videoButton.transform.SetParent(player, false);

            RectTransform rect = videoButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-42f, 42f);
            rect.sizeDelta = new Vector2(46f, 46f);

            Image background = videoButton.GetComponent<Image>();
            Image playerBackground = player.GetComponent<Image>();
            if (playerBackground != null)
            {
                background.sprite = playerBackground.sprite;
                background.type = playerBackground.type;
            }

            Button button = videoButton.GetComponent<Button>();
            button.targetGraphic = background;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.08f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.16f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.22f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.06f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            GameObject logoObject = new GameObject("YouTubeLogo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            logoObject.transform.SetParent(videoButton.transform, false);

            RectTransform logoRect = logoObject.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.pivot = new Vector2(0.5f, 0.5f);
            logoRect.anchoredPosition = Vector2.zero;
            logoRect.sizeDelta = new Vector2(26f, 26f);

            Image logo = logoObject.GetComponent<Image>();
            logo.raycastTarget = false;
            logo.preserveAspect = true;
            logo.color = new Color(1f, 1f, 1f, 0.94f);
            logo.sprite = Main.GetSpriteFromAssets("Assets/TUFHelper/Assets/Sprites/youtube_logo.png");
        }

        Button videoLinkButton = videoButton.GetComponent<Button>();
        videoLinkButton.onClick.RemoveListener(OpenPassVideoLink);
        videoLinkButton.onClick.AddListener(OpenPassVideoLink);
    }
    public async void SetPassInfo(PassesListInfoElementJson pass, LevelListInfoElementJson level)
    {
        SetVideoButton(null);
        var info = await LoadPass(pass.ID);
        if (info == null) return;
        levelIcon.sprite = Main.GetSpriteFromAssets(DiffSpriteHelper.GetSpriteFromId(level.DiffId));
        levelNameText.text = level.Song;
        compositorText.text = level.Artist;
        authorText.text = level.Creator;
        LanguageManager.ApplyChineseJapaneseFont(levelNameText);
        LanguageManager.ApplyChineseJapaneseFont(compositorText);
        LanguageManager.ApplyChineseJapaneseFont(authorText);
        chartText.text = $"Chart by: {level.Charter}";
        vfxText.text = $"VFX by: {level.Vfxer}";
        newPlayerScoreText.text = info.ScoreInfo.CurrentRankedScore.ToString("F1");
        oldPlayerScoreText.text = info.ScoreInfo.PreviousRankedScore.ToString("F1");
        addPlayerScoreText.text = "+" + info.ScoreInfo.Impact.ToString("F1");
        rankText.text = "#" + info.Ranks.RankedScoreRank.ToString();
        playerText.text = info.Player.Name;
        scorePPText.text = info.ScoreV2.ToString("F1") + "PP";

        accuracyText.text = (info.Accuracy * 100).ToString("F2") + "%";
        speedText.text = info.Speed.ToString("F2") + "x";
        feelingRating.text = info.FeelingRating;

        float countHits = pass.Judgements.Perfect + pass.Judgements.EPerfect + pass.Judgements.LPerfect +
                        pass.Judgements.EarlySingle + pass.Judgements.LateSingle + pass.Judgements.EarlyDouble;

        if (countHits <= 0) countHits = 1;

        // Perfect
        perfectsText.text = pass.Judgements.Perfect.ToString();
        SetHeight(perfectsRect, maxHeight * (pass.Judgements.Perfect / countHits));

        // Late Perfect
        lPerfectsText.text = pass.Judgements.LPerfect.ToString();
        SetHeight(lPerfectsRect, maxHeight * (pass.Judgements.LPerfect / countHits));

        // Early Perfect
        ePerfectsText.text = pass.Judgements.EPerfect.ToString();
        SetHeight(ePerfectsRect, maxHeight * (pass.Judgements.EPerfect / countHits));

        // Late
        latesText.text = pass.Judgements.LateSingle.ToString();
        SetHeight(latesRect, maxHeight * (pass.Judgements.LateSingle / countHits));

        // Early
        earliesText.text = pass.Judgements.EarlySingle.ToString();
        SetHeight(earliesRect, maxHeight * (pass.Judgements.EarlySingle / countHits));

        // Misses
        missesText.text = pass.Judgements.EarlyDouble.ToString();
        SetHeight(missesRect, maxHeight * (pass.Judgements.EarlyDouble / countHits));
        SetVideoButton(info.VideoLink);
        UITransition.AnimateDetailLoaded(this);
    }

    public string GetDefaultUrl(int passID) => $"https://api.tuforums.com/v2/database/passes/{passID}";
    private CancellationTokenSource currentRequestToken;
    public async Task<PassesListInfoElementJson> LoadPass(int id)
    {
        currentRequestToken?.Cancel();
        currentRequestToken = new CancellationTokenSource();
        CancellationToken token = currentRequestToken.Token;

        string url = GetDefaultUrl(id);
        using UnityWebRequest webRequest = UnityWebRequest.Get(url);
        webRequest.certificateHandler = new CertificateWhore();
        webRequest.timeout = 10;

        var operation = webRequest.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
            if (token.IsCancellationRequested)
            {
                webRequest.Abort();
                return null;
            }
        }

        if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            return null;

        PassesListInfoElementJson pass = JsonConvert.DeserializeObject<PassesListInfoElementJson>(webRequest.downloadHandler.text);
        return pass;    
    }
}
