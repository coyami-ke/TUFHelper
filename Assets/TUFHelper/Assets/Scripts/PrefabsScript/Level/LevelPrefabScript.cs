using DG.Tweening;
using DirectLevel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelPrefabScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Constants")]
    public float leftRect_minWidth;
    public float leftRect_maxWidth;
    [Header("UI Components")]
    public Image difficultyIcon;
    public Image background;
    public Image likeImage;
    public Image curationIcon;
    public Image favoriteImage;
    public Image leftRectangleImage;

    [Header("Text Fields")]
    //public TextMeshProUGUI idText;
    //public TextMeshProUGUI artistText;
    //public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI artistAndSongText;
    public TextMeshProUGUI creatorText;
    public TextMeshProUGUI totalClearsT;
    public TextMeshProUGUI totalClearsText;
    public TextMeshProUGUI totalLikesText;

    [Header("Buttons & Containers")]
    public GameObject scrollView;
    public GameObject folderButton;
    public GameObject favoriteButton;
    public GameObject addToFolderButton;
    public GameObject removeLevelButton;
    public GameObject visualContainer;

    [Header("Sprites")]
    public Sprite isFavoriteSprite;
    public Sprite isNotFavoriteSprite;

    [Header("Data")]
    public LevelListInfoElementJson levelInfo;

    private RectTransform _rectTransform;
    private ScrollRect _scrollRect;
    private RectTransform _viewportTransform;

    private Color _diffColor = Color.white;
    private CancellationTokenSource _selectionCts;
    private CancellationTokenSource _cdnRequestToken;

    // Corner buffers pre-allocated to avoid GC allocs in visibility checks
    private readonly Vector3[] _itemCorners = new Vector3[4];
    private readonly Vector3[] _viewportCorners = new Vector3[4];

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;

            CancelAndDisposeToken(ref _selectionCts);

            background.DOKill();
            float targetAlpha = value ? 75f / 255f : 20f / 255f;
            background.DOColor(new Color(_diffColor.r, _diffColor.g, _diffColor.b, targetAlpha), 0.35f).SetEase(Ease.OutExpo);

            _rectTransform.DOKill();
            Vector2 targetPos = new Vector2(value ? -50f : 0f, _rectTransform.anchoredPosition.y);
            _rectTransform.DOAnchorPos(targetPos, 0.3f).SetEase(Ease.OutCubic);

            //if (value)
            //{
            //    leftRectangleImage.rectTransform.DOSizeDelta(new(leftRect_maxWidth, leftRectangleImage.rectTransform.sizeDelta.y), 0.25f).SetEase(Ease.OutQuad);
            //    difficultyIcon.rectTransform.DOAnchorPosX(difficultyIcon.rectTransform.anchoredPosition.x + 64, 0.25f).SetEase(Ease.OutQuad).SetDelay(0.05f);
            //    artistAndSongText.rectTransform.DOAnchorPosX(artistAndSongText.rectTransform.anchoredPosition.x + 64, 0.25f).SetEase(Ease.OutQuad).SetDelay(0.075f);
            //    creatorText.rectTransform.DOAnchorPosX(creatorText.rectTransform.anchoredPosition.x + 64, 0.25f).SetEase(Ease.OutQuad).SetDelay(0.1f);
            //}
            //else
            //{
            //    leftRectangleImage.rectTransform.DOSizeDelta(new(leftRect_minWidth, leftRectangleImage.rectTransform.sizeDelta.y), 0.25f).SetEase(Ease.OutQuad);
            //    difficultyIcon.rectTransform.DOAnchorPosX(difficultyIcon.rectTransform.anchoredPosition.x - 64, 0.25f, true).SetEase(Ease.OutQuad).SetDelay(0.05f);
            //    artistAndSongText.rectTransform.DOAnchorPosX(artistAndSongText.rectTransform.anchoredPosition.x - 64, 0.25f).SetEase(Ease.OutQuad).SetDelay(0.075f);
            //    creatorText.rectTransform.DOAnchorPosX(creatorText.rectTransform.anchoredPosition.x - 64, 0.25f).SetEase(Ease.OutQuad).SetDelay(0.1f);
            //}

            if (value)
            {
                if (LevelInfo.instance != null) LevelInfo.instance.LoadLevelInfo(levelInfo);
                if (LeaderboardScript.instance != null) LeaderboardScript.instance.LoadPasses(levelInfo);

                _selectionCts = new CancellationTokenSource();
                _ = HandleSelectionAsync(_selectionCts.Token);
            }
        }
    }

    public bool CanPlay { get; set; } = true;
    public bool CanDownload { get; set; } = true;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateVisibility();
    }

    public void Init(GameObject scrollViewObj)
    {
        scrollView = scrollViewObj;
        if (scrollView == null) return;

        _scrollRect = scrollView.GetComponent<ScrollRect>();
        if (_scrollRect != null)
        {
            _viewportTransform = _scrollRect.viewport;
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }
    }

    public void SetLevelInfo(LevelListInfoElementJson info, int totalClears)
    {
        if (info == null) return;

        try
        {
            levelInfo = info;

            var levelOffline = Main.DownloadedLevels?.Levels?.FirstOrDefault(l => l.ID == levelInfo.ID);
            bool isOffline = levelOffline != null;

            if (folderButton != null) folderButton.SetActive(isOffline);
            if (favoriteButton != null) favoriteButton.SetActive(isOffline);
            if (addToFolderButton != null) addToFolderButton.SetActive(isOffline);
            if (removeLevelButton != null) removeLevelButton.SetActive(isOffline);

            CanDownload = !string.IsNullOrEmpty(levelInfo.DlLink);
            CanPlay = CanDownload && levelInfo.DlLink.Contains("api.tuforums.com/cdn/");

            if (curationIcon != null)
            {
                Sprite curationSprite = Main.GetSpriteFromAssets(CurationHelper.GetSpriteFromCuration(levelInfo.Curation));
                curationIcon.sprite = curationSprite;
                curationIcon.gameObject.SetActive(curationSprite != null);
            }

            //if (idText != null) idText.text = $"#{levelInfo.ID}";

            //if (artistText != null)
            //{
            //    artistText.text = levelInfo.Artist;
            //    LanguageManager.ApplyChineseJapaneseFont(artistText);
            //}

            //if (levelNameText != null)
            //{
            //    levelNameText.text = levelInfo.Song;
            //    LanguageManager.ApplyChineseJapaneseFont(levelNameText);
            //}

            if (artistAndSongText != null)
            {
                artistAndSongText.text = levelInfo.Artist + " - " + levelInfo.Song;
            }

            if (favoriteImage != null && Main.Setting?.FavoriteLevels != null)
            {
                favoriteImage.sprite = Main.Setting.FavoriteLevels.Contains(levelInfo.ID) ? isFavoriteSprite : isNotFavoriteSprite;
            }

            if (creatorText != null)
            {
                creatorText.text = !string.IsNullOrEmpty(levelInfo.Team) ? levelInfo.Team : levelInfo.Creator;
                LanguageManager.ApplyChineseJapaneseFont(creatorText);
            }

            if (difficultyIcon != null)
            {
                difficultyIcon.sprite = Main.GetSpriteFromAssets(DiffSpriteHelper.GetSpriteFromId(levelInfo.DiffId));
            }

            bool hasClears = totalClears > 0;
            if (totalClearsText != null)
            {
                //totalClearsText.gameObject.SetActive(hasClears);
                totalClearsText.text = totalClears.ToString();
            }
            

            if (totalLikesText != null) totalLikesText.text = levelInfo.Likes.ToString();

            if (ColorUtility.TryParseHtmlString(levelInfo.Difficulty?.Color, out Color parsedColor))
            {
                _diffColor = parsedColor;
            }
            else
            {
                _diffColor = Color.white;
            }

            if (background != null)
            {
                background.color = new Color(_diffColor.r, _diffColor.g, _diffColor.b, 20f / 255f);
            }

            if (leftRectangleImage != null)
            {
                leftRectangleImage.color = new Color(_diffColor.r, _diffColor.g, _diffColor.b, 128f / 255f);
            }
        }
        catch (Exception ex)
        {
            Main.Logger?.LogException(ex);
        }
    }

    public void InfoButtonClick()
    {
        if (!IsSelected)
        {
            if (LevelListScript.instance?.levelListParent != null)
            {
                var siblings = LevelListScript.instance.levelListParent.GetComponentsInChildren<LevelPrefabScript>();
                foreach (var level in siblings)
                {
                    if (level != this) level.IsSelected = false;
                }
            }

            IsSelected = true;
        }
        else
        {
            PlayButtonClick();
        }
    }

    public async Task<CDNLevelJson> GetLevelFromCDN()
    {
        CancelAndDisposeToken(ref _cdnRequestToken);
        _cdnRequestToken = new CancellationTokenSource();
        CancellationToken token = _cdnRequestToken.Token;

        string url = $"https://api.tuforums.com/v2/database/levels/{levelInfo.ID}/cdnData";

        try
        {
            using HttpResponseMessage response = await Main.Client.GetAsync(url, token);
            response.EnsureSuccessStatusCode();

            string answer = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CDNLevelJson>(answer);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (HttpRequestException ex)
        {
            Debug.LogError($"[TUFAPIRequest] Network HTTP failure at {url}: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TUFAPIRequest] Unexpected error: {ex.Message}");
            throw;
        }
    }

    private async Task HandleSelectionAsync(CancellationToken token)
    {
        try
        {
            ScrollToSelf();

            var levelOffline = Main.DownloadedLevels?.Levels?.FirstOrDefault(l => l.ID == levelInfo.ID);
            if (levelOffline != null)
            {
                string pathToFolder = LevelDownloader.GetPathToLevelFolder(
                    Main.Setting.LevelSaveFolder, levelInfo.Song, levelInfo.Artist, levelInfo.ID);

                string pathToBG = Path.Combine(pathToFolder, "bg.png");
                if (!File.Exists(pathToBG)) pathToBG = Path.Combine(pathToFolder, "bg.jpg");

                bool hasBg = File.Exists(pathToBG);
                if (SpriteLoader.instance != null)
                {
                    SpriteLoader.instance.gameObject.SetActive(hasBg);
                    if (hasBg) SpriteLoader.instance.FromFile(pathToBG);
                }

                string audioPath = Directory.EnumerateFiles(pathToFolder, "*.ogg")
                    .Concat(Directory.EnumerateFiles(pathToFolder, "*.mp3"))
                    .FirstOrDefault();

                if (audioPath != null && CustomMusicPlayer.instance != null)
                {
                    StartCoroutine(CustomMusicPlayer.instance.LoadAndPlayAudio(audioPath));
                }
            }
            else
            {
                if (CustomMusicPlayer.instance != null) CustomMusicPlayer.instance.StopPlay();
                if (SpriteLoader.instance != null) SpriteLoader.instance.gameObject.SetActive(false);

                CDNLevelJson levelFromCDN = await GetLevelFromCDN();

                if (token.IsCancellationRequested || !_isSelected) return;

                if (levelFromCDN?.Metadata?.SongFiles?.Count > 0)
                {
                    var song = levelFromCDN.Metadata.SongFiles.Values.FirstOrDefault();
                    if (song != null)
                    {
                        AudioType audioType = song.Type?.ToLower() switch
                        {
                            "ogg" => AudioType.OGGVORBIS,
                            "wav" => AudioType.WAV,
                            "mp3" => AudioType.MPEG,
                            _ => AudioType.UNKNOWN
                        };

                        if (audioType != AudioType.UNKNOWN && CustomMusicPlayer.instance != null)
                        {
                            StartCoroutine(CustomMusicPlayer.instance.PlayAudioStream(song.Url, audioType, 15));
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Main.Logger?.LogException(ex);
        }
    }

    public void PlayButtonClick()
    {
        if (CanDownload && !CanPlay)
        {
            Application.OpenURL(levelInfo.DlLink);
            return;
        }

        if (!CanDownload || !CanPlay) return;
        if (DownloadPanel.instance != null && DownloadPanel.instance.IsDownloading) return;

        if (ErrorScript.instance != null)
        {
            try { ErrorScript.instance.gameObject.SetActive(false); } catch { }
        }

        try
        {
            LevelDownloader downloader = new LevelDownloader(levelInfo)
            {
                ErrorHandler = (ex) => DirectLevel.Utils.RunAtMainThread(() => ExceptionCatch(ex))
            };

            downloader.DownloadComplete += OnCompleteDownload;
            DownloadPanel.instance.DownloadLevel(downloader);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ExceptionCatch(ex);
        }
    }

    private void OnCompleteDownload(object sender, DownloadCompleteEventArgs args)
    {
        if (args.Levels == null || args.Levels.Count == 0)
        {
            ExceptionCatch(new Exception("No valid .adofai files found in the level package."));
            return;
        }

        if (args.Levels.Count == 1)
        {
            UIScript.SwipeToBlack(() => ADOFAIGameplayHandler.OpenLevel(args.Levels[0], levelInfo));
        }
        else if (LevelSelector.instance != null)
        {
            LevelSelector.instance.LevelInfo = levelInfo;
            StartCoroutine(LevelSelector.instance.LoadLevelsCo(args.Levels, levelInfo));
        }

        ADOFAIGameplayHandler.IsFromTUFHelper = true;
        ADOFAIGameplayHandler.EditorPlayPatch.CurrentLevelInfo = levelInfo;
    }

    public void OnFavoriteButtonClicked()
    {
        if (Main.Setting?.FavoriteLevels == null) return;

        if (Main.Setting.FavoriteLevels.Contains(levelInfo.ID))
        {
            if (favoriteImage != null) favoriteImage.sprite = isNotFavoriteSprite;
            Main.Setting.FavoriteLevels.Remove(levelInfo.ID);
        }
        else
        {
            if (favoriteImage != null) favoriteImage.sprite = isFavoriteSprite;
            Main.Setting.FavoriteLevels.Add(levelInfo.ID);
        }

        Main.Setting.Save(Main.ModEntry);
    }

    public void OnAddOrRemoveFolderButtonClicked()
    {
        if (AddLevelToFolder.instance != null)
        {
            AddLevelToFolder.instance.SetInfo(levelInfo.ID);
        }
    }

    public async Task RemoveLevelAsync()
    {
        var info = Main.DownloadedLevels?.Levels?.FirstOrDefault(e => e.ID == levelInfo.ID);
        if (info != null)
        {
            Main.Setting.FavoriteLevels.Remove(levelInfo.ID);
            Main.DownloadedLevels.Levels.Remove(info);

            string path = LevelDownloader.GetPathToLevelFolder(
                Main.Setting.LevelSaveFolder, levelInfo.Song, levelInfo.Artist, levelInfo.ID);

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            if (LevelListScript.instance != null)
            {
                LevelListScript.instance.ClearLevels();
                await LevelListScript.instance.UpdateLevelListAsync();
            }
        }
    }

    public void RemoveLevel()
    {
        _ = RemoveLevelAsync();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InfoButtonClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsSelected && background != null)
        {
            background.DOKill();
            background.DOColor(new Color(_diffColor.r, _diffColor.g, _diffColor.b, 50f / 255f), 0.3f).SetEase(Ease.OutExpo);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsSelected && background != null)
        {
            background.DOKill();
            background.DOColor(new Color(_diffColor.r, _diffColor.g, _diffColor.b, 20f / 255f), 0.3f).SetEase(Ease.OutExpo);
        }
    }

    private void ScrollToSelf()
    {
        if (_scrollRect == null || _rectTransform == null) return;

        Canvas.ForceUpdateCanvases();

        RectTransform content = _scrollRect.content;
        RectTransform viewport = _scrollRect.viewport;

        float itemY = -_rectTransform.localPosition.y;
        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        if (Mathf.Approximately(contentHeight, viewportHeight)) return;

        float targetNormalizedPos = Mathf.Clamp01((itemY - (viewportHeight / 2f)) / (contentHeight - viewportHeight));
        float finalPos = 1f - targetNormalizedPos;

        _scrollRect.DOKill();
        _scrollRect.DOVerticalNormalizedPos(finalPos, 0.4f).SetEase(Ease.OutCubic);
    }

    public void OpenFolder()
    {
        FolderOpener.OpenFolder(LevelDownloader.GetPathToLevelFolder(
            Main.Setting.LevelSaveFolder, levelInfo.Song, levelInfo.Artist, levelInfo.ID));
    }

    private void Update()
    {
        // Hotkey selection play handling
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            var inputField = selected != null ? selected.GetComponent<TMP_InputField>() : null;
            bool isTyping = inputField != null && inputField.isFocused;

            if (!isTyping && IsSelected)
            {
                PlayButtonClick();
            }
        }
    }

    private void OnScrollValueChanged(Vector2 pos)
    {
        UpdateVisibility();
    }

    public bool IsVisibleInScrollView()
    {
        if (_scrollRect == null || _viewportTransform == null || _rectTransform == null)
            return true;

        _rectTransform.GetWorldCorners(_itemCorners);
        _viewportTransform.GetWorldCorners(_viewportCorners);

        return _itemCorners[1].y >= _viewportCorners[0].y && _itemCorners[0].y <= _viewportCorners[1].y;
    }

    public void UpdateVisibility()
    {
        bool isVisible = IsVisibleInScrollView();

        if (visualContainer != null)
        {
            if (visualContainer.activeSelf != isVisible)
            {
                visualContainer.SetActive(isVisible);
            }
        }
        else
        {
            if (gameObject.activeSelf != isVisible)
            {
                gameObject.SetActive(isVisible);
            }
        }
    }

    private void ExceptionCatch(Exception ex)
    {
        ErrorScript.ShowError(ex.Message);
        Main.Logger?.Error(ex.StackTrace);
    }

    private void CancelAndDisposeToken(ref CancellationTokenSource cts)
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }

    private void OnDestroy()
    {
        if (_scrollRect != null)
        {
            _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }

        CancelAndDisposeToken(ref _selectionCts);
        CancelAndDisposeToken(ref _cdnRequestToken);

        _rectTransform.DOKill();
        if (background != null) background.DOKill();
        if (_scrollRect != null) _scrollRect.DOKill();
    }
}