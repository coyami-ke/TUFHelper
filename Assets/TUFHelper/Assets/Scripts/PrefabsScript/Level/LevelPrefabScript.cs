using DG.Tweening;
using DirectLevel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using TUFHelper.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelPrefabScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }
    public void Init(GameObject scrollViewObj)
    {
        scrollView = scrollViewObj;
        _scrollRect = scrollView.GetComponent<ScrollRect>();
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            var inputField = selected != null ? selected.GetComponent<TMP_InputField>() : null;
            bool isTyping = inputField != null && inputField.isFocused;

            if (!isTyping)
            {
                if (IsSelected) PlayButtonClick();
            }
        }
    }

    private static readonly Color SelectedColor = new(1f, 1f, 1f, 50f / 255f);
    private static readonly Color DeselectedColor = new(1f, 1f, 1f, 10f / 255f);
    private RectTransform _rectTransform;
    private ScrollRect _scrollRect;
    private bool _isSelected = false;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;

            background.DOColor(value ? SelectedColor : DeselectedColor, 0.3f).SetEase(Ease.OutExpo);

            Vector2 targetPos = new(value ? -50 : 0, _rectTransform.anchoredPosition.y);
            _rectTransform.DOAnchorPos(targetPos, 0.3f).SetEase(Ease.OutBack);

            if (value)
            {
                ScrollToSelf();
                LeaderboardScript.instance.LoadPasses(levelInfo);

                var levelOffline = Main.Setting.DownloadedLevels.FirstOrDefault(l => l.LevelInfo.ID == levelInfo.ID);
                if (levelOffline != null)
                {
                    LevelInfo.instance.LoadInfoFromFile(levelOffline.LocalData);
                    string pathToBG = Path.Combine(levelOffline.NameFolder, "bg.png");
                    if (!File.Exists(pathToBG))
                        pathToBG = Path.Combine(levelOffline.NameFolder, "bg.jpg");

                    if (File.Exists(pathToBG))
                    {
                        SpriteLoader.instance.gameObject.SetActive(true);
                        SpriteLoader.instance.FromFile(pathToBG);
                    }
                    else
                    {
                        SpriteLoader.instance.gameObject.SetActive(false);
                    }

                    string oggFile = Directory.GetFiles(levelOffline.NameFolder)
                                            .FirstOrDefault(f => f.EndsWith(".ogg"));
                    if (oggFile != null)
                        StartCoroutine(CustomMusicPlayer.instance.LoadAndPlayAudio(oggFile));
                    else if (Main.Setting.PlayBackgroundMusic)
                        CustomMusicPlayer.instance.StopPlay();
                    LevelInfo.instance.IsShow = true;
                    LevelInfo.instance.LoadInfoFromFile(levelOffline.LocalData);
                }
                else
                {
                    LevelInfo.instance.IsShow = false;
                    CustomMusicPlayer.instance.StopPlay();
                    SpriteLoader.instance.gameObject.SetActive(false);

                }
            }
        }
    }

    private bool _canPlay = true;
    public bool CanPlay
    {
        get => _canPlay;
        set
        {
            _canPlay = value;
            canPlayImage.gameObject.SetActive(value);
        }
    }

    private bool _canDownload = true;
    public bool CanDownload
    {
        get => _canDownload;
        set
        {
            _canDownload = value;
            canDownloadImage.gameObject.SetActive(value);
        }
    }

    public Image difficultyIcon, background, canDownloadImage, canPlayImage;
    public TextMeshProUGUI idText,
        artistText,
        levelNameText,
        creatorText,
        totalClearsT,
        totalClearsText,
        totalLikesText;
    public GameObject scrollView;
    public GameObject folderButton, favoriteButton;
    public Image favoriteImage;
    public Sprite isFavoriteSprite, isNotFavoriteSprite;

    public LevelListInfoElementJson levelInfo;

    public void SetLevelInfo(LevelListInfoElementJson levelInfo, int totalClears)
    {
        var levelOffline = Main.Setting.DownloadedLevels.FirstOrDefault(l => l.LevelInfo.ID == levelInfo.ID);
        if (levelOffline == null)
        {
            folderButton.SetActive(false);
            favoriteButton.SetActive(false);
        }

        if (string.IsNullOrEmpty(levelInfo.DlLink))
        {
            CanDownload = false;
        }
        if (levelInfo.DlLink == null ||
            (!levelInfo.DlLink.Contains("drive.google") &&
            !levelInfo.DlLink.Contains("discord") &&
            !levelInfo.DlLink.Contains("hyonsu") &&
            !levelInfo.DlLink.Contains("api.tuforums.com/cdn/")))
        {
            CanPlay = false;
        }

        this.levelInfo = levelInfo;

        idText.text = "#" + levelInfo.ID;
        artistText.text = levelInfo.Artist;
        levelNameText.text = levelInfo.Song;

        if (Main.Setting.FavoriteLevels.Contains(levelInfo.ID))
        {
            this.favoriteImage.sprite = isFavoriteSprite;
        }
        else
        {
            this.favoriteImage.sprite = isNotFavoriteSprite;
        }

        if (!string.IsNullOrEmpty(levelInfo.Team))
        {
            creatorText.text = levelInfo.Team;
        }
        else
        {
            creatorText.text = levelInfo.Creator;
        }

        difficultyIcon.sprite = Main.assets.LoadAsset<Sprite>(DiffSpriteHelper.GetSpriteFromId(levelInfo.DiffId));

        if (totalClears == 0)
        {
            totalClearsT.gameObject.SetActive(false);
            totalClearsText.gameObject.SetActive(false);
        }
        else
        {
            totalClearsT.gameObject.SetActive(true);
            totalClearsText.gameObject.SetActive(true);

            totalClearsText.text = "" + totalClears;
        }

        totalLikesText.text = levelInfo.Likes.ToString();
    }

    public void InfoButtonClick()
    {
        LeaderboardScript.instance.LoadPasses(levelInfo);

        if (!IsSelected)
        {
            foreach (var level in LevelListScript.instance.levelListParent.GetComponentsInChildren<LevelPrefabScript>())
            {
                if (level != this)
                    level.IsSelected = false;
            }

            IsSelected = true;
        }
        else
        {
            PlayButtonClick();
        }
    }


    private void ExceptionCatch(Exception ex)
    {
        ErrorScript.ShowError(ex.Message);
    }

    public void PlayButtonClick()
    {
        if (CanDownload && !CanPlay) Application.OpenURL(levelInfo.DlLink);
        if (!CanDownload || !CanPlay) return;
        if (DownloadPanel.instance.IsDownloading) return;

        ErrorScript.instance.gameObject.SetActive(false);

        try
        {

            LevelDownloader levelDownloder = new(levelInfo.DlLink)
            {
                ErrorHandler = (ex) =>
                {
                    DirectLevel.Utils.RunAtMainThread(() => ExceptionCatch(ex));
                }
            };

            DownloadPanel.instance.DownloadLevel(levelDownloder);

            levelDownloder.DownloadComplete += OnCompleteDownload;

        }
        catch (Exception ex)
        {
            ExceptionCatch(ex);
        }
    }
    private void OnCompleteDownload(object sender, DownloadCompleteEventArgs args)
    {
        switch (args.Levels.Count)
        {
            case 0:
                throw new Exception("adofai file was not found");
            case 1:
                SaveLevelToSettings(levelInfo, Path.GetDirectoryName(args.Levels[0]), args.Levels[0]);
                UIScript.SwipeToBlack(() => TryToLoadLevel(args.Levels[0]));
                break;
            default:
                SaveLevelToSettings(levelInfo, Path.GetDirectoryName(args.Levels[0]), args.Levels[0]);
                StartCoroutine(LevelSelector.instance.LoadLevelsCo(args.Levels));
                break;
        }
    }
    private void SaveLevelToSettings(LevelListInfoElementJson levelJson, string folder, string saveableLevel)
    {
        foreach (var level in Main.Setting.DownloadedLevels.ToArray())
        {
            if (level.LevelInfo.ID == levelJson.ID)
            {
                Main.Setting.DownloadedLevels.Remove(level);
            }
        }
        LevelData levelData;
        try
        {
            string rawJson = File.ReadAllText(saveableLevel);

            // Fix missing comma between JSON arrays/objects
            string pattern = @"\](\s*)""decorations""";
            string replacement = "],$1\"decorations\"";

            if (Regex.IsMatch(rawJson, pattern))
            {
                rawJson = Regex.Replace(rawJson, pattern, replacement);
                Main.Logger.Log("the level json has been fixed");
            }

            levelData = JsonConvert.DeserializeObject<LevelData>(rawJson);
        }
        catch (JsonReaderException jsonEx)
        {
            Main.Logger.Error(jsonEx.Message);
            return;
        }

        float bpm = levelData.Settings.BPM;
        string pathDataString = levelData.GetPathDataAsString();
        float[] pathDataArray = levelData.GetPathDataAsFloatArray();

        int countTiles = 0;
        if (pathDataArray != null)
        {
            countTiles = pathDataArray.Length;
        }
        else if (pathDataString != null)
        {
            countTiles = pathDataString.Length;
        }

        var oggs = Directory.GetFiles(folder).Where(f => f.EndsWith(".ogg")).OrderByDescending(e => new FileInfo(e).Length).ToArray();

        string oggFile = oggs[0];
        if (oggFile != null)
        {
            StartCoroutine(GetOggLength(oggFile, length =>
            {
                var localData = new CustomLevelInfoJson
                {
                    BPM = bpm,
                    Tiles = countTiles,
                    Lenght = length
                };

                Main.Setting.DownloadedLevels.Add(new()
                {
                    LevelInfo = levelJson,
                    NameFolder = folder,
                    LocalData = localData
                });
            }));
        }

        Main.Setting.Save(Main.ModEntry);
        Main.Logger.Log($"The level has been saved in the folder");
    }
    private IEnumerator GetOggLength(string path, Action<float> onLengthReceived)
    {
        string url = "file:///" + path.Replace("\\", "/");
        using UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error loading .ogg: " + uwr.error);
            onLengthReceived?.Invoke(0f);
        }
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
            onLengthReceived?.Invoke(clip.length);
        }
    }
    public void OnFavoriteButtonClicked()
    {
        if (Main.Setting.FavoriteLevels.Contains(levelInfo.ID))
        {
            favoriteImage.sprite = isNotFavoriteSprite;
            Main.Setting.FavoriteLevels.Remove(levelInfo.ID);
        }
        else
        {
            favoriteImage.sprite = isFavoriteSprite;
            Main.Setting.FavoriteLevels.Add(levelInfo.ID);
        }
        Main.Setting.Save(Main.ModEntry);
    }


    public static void TryToLoadLevel(string levelFilePath)
    {
        //DownloadPopupScript.IsDownloading = false;
        HideUIFixPatch.RecentDirectLevelOpend = true;

        GCS.sceneToLoad = "scnEditor";
        GCS.worldEntrance = null;
        scnEditor.levelToOpenOnLoad = levelFilePath;

        SceneManager.LoadScene("scnEditor");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InfoButtonClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsSelected) background.DOColor(new Color(1f, 1f, 1f, 20 / 255f), 0.5f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsSelected) background.DOColor(new Color(1f, 1f, 1f, 10 / 255f), 0.5f).SetEase(Ease.OutExpo);
    }

    private void ScrollToSelf()
    {
        if (_scrollRect == null || _rectTransform == null) return;

        Canvas.ForceUpdateCanvases();

        RectTransform content = _scrollRect.content;
        RectTransform viewport = _scrollRect.viewport;

        Vector2 itemLocalPos = _rectTransform.localPosition;
        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        float itemY = -itemLocalPos.y;

        float targetNormalizedPos = Mathf.Clamp01((itemY - (viewportHeight / 2)) / (contentHeight - viewportHeight));

        float finalPos = 1 - targetNormalizedPos;

        _scrollRect.DOVerticalNormalizedPos(finalPos, 0.5f).SetEase(Ease.OutCubic);
    }
    public void OpenFolder()
    {
        var levelOffline = Main.Setting.DownloadedLevels.FirstOrDefault(l => l.LevelInfo.ID == levelInfo.ID);
        if (levelOffline == null) return;
        FolderOpener.OpenFolder(levelOffline.NameFolder);
    }
}
internal class LevelData
{
    [JsonProperty("pathData")]
    public JToken PathDataRaw { get; set; }

    [JsonProperty("settings")]
    public LevelSettings Settings { get; set; }

    public string GetPathDataAsString()
    {
        return PathDataRaw?.Type == JTokenType.String ? PathDataRaw.ToString() : null;
    }

    public float[] GetPathDataAsFloatArray()
    {
        if (PathDataRaw?.Type == JTokenType.Array)
        {
            return PathDataRaw.ToObject<float[]>();
        }
        return null;
    }
}

internal class LevelSettings
{
    [JsonProperty("bpm")]
    public float BPM { get; set; }
}