using System;
using TMPro;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

[RegisterIngameElement("LevelInfo", "assets/tufhelper/assets/prefabs/IngameLevelInfoPrefab.prefab")]
public class IngameLevelInfoScript : BasicIngameElement
{
    public TextMeshProUGUI artistText, songText;
    public Image diffIcon;

    public override string ID => "LevelInfo";
    public override string NameInSettings => "Level Info";
    public override Sprite Icon => Main.assets.LoadAsset<Sprite>("assets/tufhelper/assets/sprites/info.png");
    public override bool IsShownOnlyInTUFHelper => true;
    public override Anchor DefaultAnchor => Anchor.RightTop;

    #region Self-Contained Gameplay Event Hooks

    protected override void OnPlay(PlayButtonEventArgs e)
    {
        if (e.CurrentLevelInfo != null)
        {
            SetLevelInfo(e.CurrentLevelInfo);
        }
    }

    #endregion

    #region UI Presentation Logic

    public void SetLevelInfo(LevelListInfoElementJson levelInfo)
    {
        artistText.text = "#" + levelInfo.ID + " " + levelInfo.Artist;
        songText.text = levelInfo.Song;

        LanguageManager.ApplyChineseJapaneseFont(artistText);
        LanguageManager.ApplyChineseJapaneseFont(songText);

        if (diffIcon != null)
        {
            diffIcon.sprite = Main.GetSpriteFromAssets(DiffSpriteHelper.GetSpriteFromId(levelInfo.DiffId));
        }
    }

    #endregion
}