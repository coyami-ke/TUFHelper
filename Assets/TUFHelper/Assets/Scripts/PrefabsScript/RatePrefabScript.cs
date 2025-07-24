using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper.AccountSystem;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

public class RatePrefabScript : MonoBehaviour
{
    public TextMeshProUGUI artistSong, yourRating, managerRating, communityRating, supposedRate, rerateMessage, levelID;

    public Image background;

    public RatingElementJson RatingInfo { get; private set; }

    public void SetRateInfo(RatingElementJson info)
    {
        artistSong.text = $"{info.Level.Artist} - {info.Level.Song}";
        if (info.CurrentDifficulty != null) managerRating.text = info.CurrentDifficulty.Name;
        else managerRating.text = "?";
        if (info.CommunityDifficulty != null) communityRating.text = info.CommunityDifficulty.Name;
        else communityRating.text = "?";
        levelID.text = "#" + info.LevelID.ToString();

        rerateMessage.text = info.Level.RerateReason;

        if (info?.RequestedDiffID < 20) background.color = new(0.25f, 1, 0.25f, 50 / 255f);
        else if (info.Details.Count >= 4) background.color = new(1, 0.25f, 0.25f, 50 / 255f);

        RatingInfo = info;
    }
    public void DownloadLevel()
    {
        SearchScript.instance.searchField.text = "#" + RatingInfo.LevelID.ToString();
        SearchScript.instance.OnEndEdit("#" + RatingInfo.LevelID.ToString());

        var levels = LevelListScript.instance.GetLevelPrefabScripts();
        if (levels.Length > 0) levels[0].IsSelected = true;
    }
}
