using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

public class IngameLevelInfoScript : MonoBehaviour
{
    public static IngameLevelInfoScript Instance { get; private set; }

    public TextMeshProUGUI artistText, songText;
    public Image diffIcon;

    public void Awake()
    {
        Instance = this;
    }

    public void SetLevelInfo(LevelListInfoElementJson levelInfo)
    {
        artistText.text = "#" + levelInfo.ID + " " + levelInfo.Artist;
        songText.text = levelInfo.Song;
        LanguageManager.ApplyChineseJapaneseFont(artistText);
        LanguageManager.ApplyChineseJapaneseFont(songText);

        diffIcon.sprite = Main.GetSpriteFromAssets(DiffSpriteHelper.GetSpriteFromId(levelInfo.DiffId));
    }
}
