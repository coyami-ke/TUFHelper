using DirectLevel;
using System;
using TMPro;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

public class LevelPrefabScript : MonoBehaviour
{

    public Image difficultyIcon;
    public Button watchButton, downloadButton, playButton;
    public TextMeshProUGUI idText, artistText, levelNameText, creatorText, watchButtonText, downloadButtonText, playButtonText;

    public LevelInfo levelInfo;

    public void Awake()
    {
        
    }

    public void Update()
    {
        
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

        if (!levelInfo.dlLink.Contains("drive.google") && !levelInfo.dlLink.Contains("discord") && !levelInfo.dlLink.Contains("hyonsu"))
        {
            playButton.interactable = false;
            playButtonText.color = new Color(150 / 255f, 150 / 255f, 150 / 255f);
        }
    }

    public void WatchButtonClick()
    {
        Application.OpenURL(levelInfo.vidLink);
    }

    public void DownloadButtonClick()
    {
        Application.OpenURL(levelInfo.dlLink);
    }

    public void PlayButtonClick()
    {
        try
        {
            Persistence.SetHideCursorWhilePlaying(false);
            DirectLevelAPI.PlayFromID(DirectLevelAPI.ForumType.T21C, levelInfo.id + "", true, true);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            ErrorScript.ShowError(ex.Message);
        }
    }

}
