using DirectLevel;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelInfoSceneScript : MonoBehaviour
{

    public static LevelInfo currentLevelInfo;

    public Image difficultyIcon;
    public Button downloadButton, playButton;

    public TextMeshProUGUI 
        songText,
        creatorText,
        downloadButtonText,
        playButtonText;

    // Start is called before the first frame update
    public void Awake()
    {
        void ClickSfx()
        {
            scrSfx.instance?.PlaySfx(SfxSound.MobileButton);
        }

        downloadButton.onClick.AddListener(ClickSfx);
        playButton.onClick.AddListener(ClickSfx);

        difficultyIcon.sprite = Helper.getDiffSprite(currentLevelInfo.pguDiff);
        songText.text = currentLevelInfo.artist + " - " + currentLevelInfo.song;
        creatorText.text = "Level By: " + currentLevelInfo.creator;

/*        if (currentLevelInfo.vidLink.Equals(""))
        {
            watchButton.interactable = false;
            watchButtonText.color = new Color(150 / 255f, 150 / 255f, 150 / 255f);
        }*/

        if (currentLevelInfo.dlLink.Equals(""))
        {
            downloadButton.interactable = false;
            downloadButtonText.color = new Color(150 / 255f, 150 / 255f, 150 / 255f);
        }

        if (!currentLevelInfo.dlLink.Contains("drive.google") && !currentLevelInfo.dlLink.Contains("discord") &&
            !currentLevelInfo.dlLink.Contains("hyonsu"))
        {
            playButton.interactable = false;
            playButtonText.color = new Color(150 / 255f, 150 / 255f, 150 / 255f);
        }
    }


    public void DownloadButtonClick()
    {
        if (DownloadPopupScript.IsDownloading) return;

        Application.OpenURL(currentLevelInfo.dlLink);
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

        ErrorScript.instance.gameObject.SetActive(false);

        try
        {
            DownloadPopupScript.IsDownloading = true;

            //Persistence.SetHideCursorWhilePlaying(false);

            DownloadPopupScript.Show();

            var levelDownloder = new LevelDownloader(currentLevelInfo.dlLink);

            levelDownloder.ErrorHandler = (ex) =>
            {
                DirectLevel.Utils.RunAtMainThread(() => ExceptionCatch(ex));
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
                        UIScript.SwipeToBlack(() => TryToLoadLevel(levelList[0]));
                        break;
                    default:
                        DirectLevel.Utils.RunAtMainThread(() =>
                        {
                            DownloadPopupScript.Close();
                            ResultLevelScript.ShowList(levelList);
                        });
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
                    return true;
                }

                return false;


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