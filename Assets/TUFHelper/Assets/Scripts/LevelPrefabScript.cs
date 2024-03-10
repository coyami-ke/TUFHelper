using DirectLevel;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
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
        if (DirectLevelAPI.IsDownloading) return;
        
        Application.OpenURL(levelInfo.vidLink);
    }

    public void DownloadButtonClick()
    {
        if (DirectLevelAPI.IsDownloading) return;
        
        Application.OpenURL(levelInfo.dlLink);
    }

    public void PlayButtonClick()
    {
        if (DirectLevelAPI.IsDownloading) return;
            
        try
        {
            var levelName = new Regex("[^a-zA-Z0-9 -]").Replace(levelInfo.song.ToLower(), string.Empty);

            Persistence.SetHideCursorWhilePlaying(false);



            DownloadPopupScript.Show();
            DirectLevelAPI.PlayFromIDTask(DirectLevelAPI.ForumType.TUC, levelInfo.id + "", true, levelName, true,
                (ex) =>
                {
                    Main.mainThread.Post(_ =>
                    {
                        UIScript.SwipeFromBlack();

                        Debug.LogException(ex);
                        ErrorScript.ShowError(ex.Message);
                        
                        DirectLevelAPI.IsDownloading = false;
                    },null);
                });
            
        }
        catch (Exception ex)
        {

            UIScript.SwipeFromBlack();

            Debug.LogException(ex);
            ErrorScript.ShowError(ex.Message);
            
            DirectLevelAPI.IsDownloading = false;
        }
    }

}
