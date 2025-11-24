using DG.Tweening;
using DirectLevel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
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
                try
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
                        string mp3File = Directory.GetFiles(levelOffline.NameFolder)
                                                  .FirstOrDefault(f => f.EndsWith(".mp3"));

                        if (oggFile != null)
                            StartCoroutine(CustomMusicPlayer.instance.LoadAndPlayAudio(oggFile));
                        else if (mp3File != null)
                            StartCoroutine(CustomMusicPlayer.instance.LoadAndPlayAudio(mp3File));
                        //else if (Main.Setting.PlayBackgroundMusic)
                        //    CustomMusicPlayer.instance.StopPlay();
                    }
                    else
                    {
                        LevelInfo.instance.IsShow = false;
                        CustomMusicPlayer.instance.StopPlay();
                        SpriteLoader.instance.gameObject.SetActive(false);

                    }
                }
                catch (Exception ex)
                {
                    Main.Logger.LogException(ex);
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

    public Image difficultyIcon, background, canDownloadImage, canPlayImage, likeImage, curationIcon;
    public TextMeshProUGUI idText,
        artistText,
        levelNameText,
        creatorText,
        totalClearsT,
        totalClearsText,
        totalLikesText;
    public GameObject scrollView;
    public GameObject folderButton, favoriteButton, addToFolderButton, removeLevelButton;
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
            addToFolderButton.SetActive(false);
            removeLevelButton.SetActive(false);
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
        if (levelInfo.Curation != null)
        {
            curationIcon.sprite = Main.assets.LoadAsset<Sprite>(CurationHelper.GetSpriteFromId(levelInfo.Curation.TypeID));
        }
        else
        {
            curationIcon.gameObject.SetActive(false);
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
            //sWindowsManager.instance.ShowPassList();
        }
        else
        {
            PlayButtonClick();
        }
    }

    private void ExceptionCatch(Exception ex)
    {
        ErrorScript.ShowError(ex.Message);
        Main.Logger.Error(ex.StackTrace);
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
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ExceptionCatch(ex);
        }
    }
    private void OnCompleteDownload(object sender, DownloadCompleteEventArgs args)
    {
        // Donates Current Levelinfo to PPDisplayer & notify that play mode is from TUFHelper
        // PPDisplayerPatch.Levelinfo = levelInfo;
        // PPDisplayerPatch.IsFromTUFH = true;

        switch (args.Levels.Count)
        {
            case 0:
                throw new Exception("adofai file was not found");
            case 1:
                SaveLevelToSettings(levelInfo, Path.GetDirectoryName(args.Levels[0]), args.Levels[0]);
                UIScript.SwipeToBlack(() => TryToLoadLevel(levelInfo, args.Levels[0]));
                break;
            default:
                SaveLevelToSettings(levelInfo, Path.GetDirectoryName(args.Levels[0]), args.Levels[0]);
                LevelSelector.instance.LevelInfo = this.levelInfo;
                StartCoroutine(LevelSelector.instance.LoadLevelsCo(args.Levels));
                break;
        }

        // IngameLeaderboardPatch.LevelInfo = this.levelInfo;
        // IngameLeaderboardPatch.IsInTUFHelper = true;

        ADOFAIGameplayHandler.IsFromTUFHelper = true;
        ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo = levelInfo;
    }
    public static void SaveLevelToSettings(LevelListInfoElementJson levelJson, string folder, string saveableLevel)
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

            // Fix double comma in requiredMods
            string fixRequiredModsDoubleComma = @"""requiredMods""\s*:\s*\[\]\s*,\s*,";
            rawJson = Regex.Replace(rawJson, fixRequiredModsDoubleComma, @"""requiredMods"": [],");

            // Existing fixes
            string fixMissingCommas = @"}\s*{";
            rawJson = Regex.Replace(rawJson, fixMissingCommas, "},\n{");

            string pattern = @"\](\s*)""decorations""";
            string replacement = "],$1\"decorations\"";
            rawJson = Regex.Replace(rawJson, pattern, replacement);


            // Попытка десериализации
            levelData = JsonConvert.DeserializeObject<LevelData>(rawJson);

            Main.Logger.Log("Level loaded and JSON fixed if necessary.");
        }
        catch (JsonReaderException jsonEx)
        {
            Main.Logger.Error("JSON parse error: " + jsonEx.Message);
            return;
        }

        float bpm = levelData.Settings.BPM;
        int pathData = levelData.PathData.Length;
        int angleData = levelData.AngleData.Count;

        int countTiles = pathData + angleData;

        var oggs = Directory.GetFiles(folder)
            .Where(f => f.EndsWith(".ogg"))
            .OrderByDescending(e => new FileInfo(e).Length)
            .ToArray();

        string oggFile = oggs.FirstOrDefault();
        CustomLevelInfoJson localInfo = new();

        if (!string.IsNullOrEmpty(oggFile) && LevelListScript.instance != null)
        {
            LevelListScript.instance.StartCoroutine(GetOggLength(oggFile, length =>
            {
                localInfo.Lenght = length;
                FinalizeLevelInfo();
            }));
        }
        else
        {
            localInfo.Lenght = 0;
            FinalizeLevelInfo();
        }

        void FinalizeLevelInfo()
        {
            localInfo.BPM = bpm;
            localInfo.Tiles = countTiles;

            Main.Setting.DownloadedLevels.Add(new()
            {
                LevelInfo = levelJson,
                NameFolder = folder,
                LocalData = localInfo
            });

            Main.Setting.Save(Main.ModEntry);
            Main.Logger.Log($"The level has been saved in the folder");
        }
    }
    public static IEnumerator GetOggLength(string path, Action<float> onLengthReceived)
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
    public void OnAddOrRemoveFolderButtonClicked()
    {
        AddLevelToFolder.instance.SetInfo(levelInfo.ID);
    }
    public async void RemoveLevel()
    {
        var info = Main.Setting.DownloadedLevels.FirstOrDefault(e => e.LevelInfo.ID == this.levelInfo.ID);
        if (info != null)
        {
            if (Directory.Exists(info.NameFolder))
            {
                Directory.Delete(info.NameFolder, true);
            }

            Main.Setting.FavoriteLevels.Remove(this.levelInfo.ID);
            Main.Setting.DownloadedLevels.Remove(info);
            LevelListScript.instance.ClearLevels();
            await LevelListScript.instance.UpdateLevelListAsync();
        }
    }
    public void TryLikeLevel()
    {
        
    }
    public static void TryToLoadLevel(LevelListInfoElementJson levelInfo, string levelFilePath = null)
    {
        HideUIFixPatch.RecentDirectLevelOpend = true;

        GCS.sceneToLoad = "scnEditor";
        GCS.worldEntrance = null;
        scnEditor.levelToOpenOnLoad = levelFilePath;

        ADOFAIGameplayHandler.IsFromTUFHelper = true;

        ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo = levelInfo;

        try
        {
            var account = AccountSaver.GetAccount();
            //if (account != null && account.IsRatingMode)
            //{
            //    ADOFAIGameplayHandler.EditorPlayPatch.RatingMode = account.IsRatingMode;
            //    ADOFAIGameplayHandler.EditorPlayPatch.CurrentRating = RatingPanel.instance.RatingElements.FirstOrDefault(e => e.Level.ID == levelInfo.ID);
            //}
        }
        catch
        {
            ADOFAIGameplayHandler.EditorPlayPatch.RatingMode = false;
            ADOFAIGameplayHandler.EditorPlayPatch.CurrentRating = null;
        }

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
    public string PathData { get; set; } = "";
    [JsonProperty("angleData")]
    public List<float> AngleData { get; set; } = new();

    [JsonProperty("settings")]
    public LevelSettings Settings { get; set; }
}

internal class LevelSettings
{
    [JsonProperty("bpm")]
    public float BPM { get; set; }
}

public static class SceneLoaderExtensions
{
    public static Task AsTask(this AsyncOperation asyncOp)
    {
        var tcs = new TaskCompletionSource<bool>();
        asyncOp.completed += _ => tcs.SetResult(true);
        return tcs.Task;
    }
}