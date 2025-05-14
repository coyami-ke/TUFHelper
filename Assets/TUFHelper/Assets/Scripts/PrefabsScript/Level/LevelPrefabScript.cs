using DG.Tweening;
using DirectLevel;
using System;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using TUFHelper.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
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
        totalClearsText;
    public GameObject scrollView;

    public LevelListInfoElementJson levelInfo;

    public void SetLevelInfo(LevelListInfoElementJson levelInfo, int totalClears)
    {
        if (string.IsNullOrEmpty(levelInfo.DlLink))
        {
            CanDownload = false;
        }
        if (levelInfo.DlLink == null ||
            (!levelInfo.DlLink.Contains("drive.google") &&
            !levelInfo.DlLink.Contains("discord") &&
            !levelInfo.DlLink.Contains("hyonsu")))
        {
            CanPlay = false;
        }

        this.levelInfo = levelInfo;

        idText.text = "#" + levelInfo.ID;
        artistText.text = levelInfo.Artist;
        levelNameText.text = levelInfo.Song;
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
    }

    public void InfoButtonClick()
    {
        LeaderboardScript.instance.LoadPasses(levelInfo.ID);
        if (!IsSelected) 
        {
            foreach (var level in LevelListScript.instance.levelListParent.GetComponentsInChildren<LevelPrefabScript>()) 
            {
                if (level != this)
                {
                    level.IsSelected = false;
                }
            }

            IsSelected = true;
            LeaderboardScript.instance.LoadPasses(levelInfo.ID);
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
                SaveLevelToSettings(levelInfo, Path.GetDirectoryName(args.Levels[0]));
                UIScript.SwipeToBlack(() => TryToLoadLevel(args.Levels[0]));
                break;
            default:
                SaveLevelToSettings(levelInfo, Path.GetDirectoryName(args.Levels[0]));
                StartCoroutine(LevelSelector.instance.LoadLevelsCo(args.Levels));
                break;
        }
    }
    private void SaveLevelToSettings(LevelListInfoElementJson levelJson, string folder)
    {
        foreach (var level in Main.Setting.DownloadedLevels.ToArray())
        {
            if (level.LevelInfo.ID == levelJson.ID) 
            {
                Main.Setting.DownloadedLevels.Remove(level);
            }
        }
        Main.Setting.DownloadedLevels.Add(new() { LevelInfo = levelJson, NameFolder = folder } );
        Main.Setting.Save(Main.ModEntry);
        Main.Logger.Log($"The level has been saved in the folder");
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
}
