using DirectLevel;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class LevelPrefabScript : MonoBehaviour
{

    public Image difficultyIcon;
    public TextMeshProUGUI artistText, levelNameText, creatorText;
    public Button watchButton, downloadButton, playButton;

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

        artistText.text = levelInfo.artist;
        levelNameText.text = levelInfo.song;
        creatorText.text = levelInfo.creator;

        if (Main.assets != null)
        {
            Sprite sprite = Main.assets.LoadAsset<Sprite>("Assets/TUFHelper/Assets/Sprites/DiffIcons/" + levelInfo.pgu_diff + ".png");

            if (sprite == null)
            {
                sprite = Main.assets.LoadAsset<Sprite>("Assets/TUFHelper/Assets/Sprites/DiffIcons/unknown.png");
            }

            difficultyIcon.sprite = sprite;
        }
        else
        {
            Sprite sprite = Resources.Load<Sprite>("DiffIcons/" + levelInfo.pgu_diff);

            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>("DiffIcons/unknown");
            }

            difficultyIcon.sprite = sprite;
        }

        watchButton.interactable = !levelInfo.vidLink.Equals("");
        downloadButton.interactable = !levelInfo.dlLink.Equals("");
        playButton.interactable = !levelInfo.dlLink.Equals("");
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
        DirectLevelAPI.PlayFromID(DirectLevelAPI.ForumType.T21C, levelInfo.id + "", true);
    }

}
