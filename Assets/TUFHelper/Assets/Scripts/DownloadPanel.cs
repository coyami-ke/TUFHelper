using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using DirectLevel;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Web;
using UnityEngine;
using UnityEngine.UI;

public class DownloadPanel : MonoBehaviour
{
    public static DownloadPanel instance;
    public bool IsDownloading { get; private set; } = false;

    private bool _isShow = false;
    public bool IsShow
    {
        get => _isShow;
        set
        {
            _isShow = value;
            if (value)
            {
                rectTransform.DOScaleY(1, 0.4f).SetEase(Ease.OutBack);
                rectTransform.DOAnchorPosY(maxPositonY, 0.4f).SetEase(Ease.OutBack);
            }
            else
            {
                rectTransform.DOScaleY(0, 0.4f).SetEase(Ease.InBack);
                rectTransform.DOAnchorPosY(minPositionY, 0.4f).SetEase(Ease.InBack);
            }
        }
    }
    public Image animIcon;
    public Sprite networkSprite, downloadSprite, zipSprite;
    public TextMeshProUGUI currentState, total, received;
    public RectTransform rectTransform;
    
    private float minPositionY = -450;
    private float maxPositonY = -410;

    public void Awake()
    {
        if (instance == null) 
            instance = this;
    }
    public void Start()
    {
        rectTransform.ScaleY(0);
        rectTransform.AnchorPosY(minPositionY);
    }

    public async void DownloadLevel(LevelDownloader downloader)
    {
        IsShow = true;
        IsDownloading = true;
        downloader.UpdateProgress += OnUpdateProgress;
        await downloader.DownloadWithTask(Main.Setting.LevelSaveFolder, true);
        IsDownloading = false;
        IsShow = false;
    }

    private void OnUpdateProgress(object sender, UpdateProgressEventArgs args)
    {
        received.text = $"{args.BytesReceived / 1024d / 1024d:F1} MB";
        total.text = $"{args.TotalBytesToReceive / 1024d / 1024d:F1} MB";


        switch (args.State)
        {
            case LevelDownloaderStates.Preparing:
                animIcon.sprite = networkSprite;
                currentState.text = "Preparing";
                break;
            case LevelDownloaderStates.Downloading:
                animIcon.sprite = downloadSprite;
                currentState.text = "Downloading";
                break;
            case LevelDownloaderStates.Unzipping:
                animIcon.sprite = zipSprite;
                currentState.text = "Unzipping";
                break;
            case LevelDownloaderStates.Downloaded:
                currentState.text = "Complete";
                StartCoroutine(HidePanelWithDelay(2f));
                break;
        }
    }

    private IEnumerator HidePanelWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        IsShow = false;
    }
}
