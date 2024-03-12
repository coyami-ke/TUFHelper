using DirectLevel;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelPrefabScript : MonoBehaviour
{

    public Image difficultyIcon;
    public Button watchButton, downloadButton, playButton;

    public TextMeshProUGUI idText,
        artistText,
        levelNameText,
        creatorText,
        watchButtonText,
        downloadButtonText,
        playButtonText;

    public LevelInfo levelInfo;

    public void Awake()
    {
        void ClickSfx()
        {
            scrSfx.instance?.PlaySfx(SfxSound.MobileButton);
        }

        watchButton.onClick.AddListener(ClickSfx);
        downloadButton.onClick.AddListener(ClickSfx);
        playButton.onClick.AddListener(ClickSfx);
    }
    

    public void SetLevelInfo(LevelInfo levelInfo)
    {
        this.levelInfo = levelInfo;

        idText.text = "#" + levelInfo.id;
        artistText.text = levelInfo.artist;
        levelNameText.text = levelInfo.song;
        creatorText.text = levelInfo.creator;

        difficultyIcon.sprite = Helper.getDiffSprite(levelInfo.pgu_diff);

        if (levelInfo.vidLink.Equals(""))
        {
            watchButton.interactable = false;
            watchButtonText.color = new Color(150 / 255f, 150 / 255f, 150 / 255f);
        }

        if (levelInfo.dlLink.Equals(""))
        {
            downloadButton.interactable = false;
            downloadButtonText.color = new Color(150 / 255f, 150 / 255f, 150 / 255f);
        }

        if (!levelInfo.dlLink.Contains("drive.google") && !levelInfo.dlLink.Contains("discord") &&
            !levelInfo.dlLink.Contains("hyonsu"))
        {
            playButton.interactable = false;
            playButtonText.color = new Color(150 / 255f, 150 / 255f, 150 / 255f);
        }
    }

    public void WatchButtonClick()
    {
        if (DownloadPopupScript.IsDownloading) return;
        
        Application.OpenURL(levelInfo.vidLink);
    }

    public void DownloadButtonClick()
    {
        if (DownloadPopupScript.IsDownloading) return;
        
        Application.OpenURL(levelInfo.dlLink);
    }

    private void ExceptionCatch(Exception ex)
    {
        
        Debug.LogException(ex);
        ErrorScript.ShowError(ex.Message);
            
        DownloadPopupScript.IsDownloading = false;
    }

    public void PlayButtonClick()
    {
        if (DownloadPopupScript.IsDownloading) return;
            
        try
        {
            DownloadPopupScript.IsDownloading = true;
            
            Persistence.SetHideCursorWhilePlaying(false);
            
            DownloadPopupScript.Show();
            
            var levelDownloder = new LevelDownloader(levelInfo.dlLink);
            
            levelDownloder.ErrorHandler = (ex)=>
            {
                DirectLevel.Utils.RunAtMainThread(()=>ExceptionCatch(ex));
            };
            
            levelDownloder.OnUpdateProgress = (progress, stateMessage) =>
            {
                DownloadPopupScript.ChangeProgress = progress;
                DownloadPopupScript.ChangeMessage = stateMessage;
            };
            
            levelDownloder.OnDownloadComplete = levelList =>
            {
                switch (levelList.Count)
                {
                    case 0:
                        throw new Exception("adofai file was not found");
                    case 1:
                        UIScript.SwipeToBlack(()=>TryToLoadLevel(levelList[0]));
                        break;
                    default:
                        // show adofai file select window
                        break;
                }
            };
            
            levelDownloder.OnCalculationCompleteFileSize = size =>
            {
                if (size > 300000000)
                {
                    DirectLevel.Utils.RunAtMainThread(() =>
                    {
                        DownloadPopupScript.ShowFileWarning(size,
                            () => { levelDownloder.DownloadWithTask(Main.Setting.levelSaveFolder, false); },
                            () => { DownloadPopupScript.IsDownloading = false; });
                    });
                }

                return true;
            };
            
            levelDownloder.DownloadWithTask(Main.Setting.levelSaveFolder, true);

        }
        catch (Exception ex)
        {
            ExceptionCatch(ex);
        }
    }
    
    
    public static void TryToLoadLevel(string levelFilePath)
    {
        DownloadPopupScript.IsDownloading = false;
        HideUIFixPatch.RecentDirectLevelOpend = true;
            
        GCS.sceneToLoad = "scnEditor";
        GCS.worldEntrance = null;
        scnEditor.levelToOpenOnLoad = levelFilePath;
        
        SceneManager.LoadScene("scnEditor");
        
        /*
        DirectLevel.Utils.RunAtMainThread(() =>
        {
            SceneManager.LoadScene("scnEditor");
        });*/
    }

}
