using System.Collections;
using System.Collections.Generic;
using TMPro;
using TUFHelper.AccountSystem;
using TUFHelper.Utils;
using UnityEngine;

public class RatePrefabScript : MonoBehaviour
{
    public TextMeshProUGUI artistSong, yourRating, managerRating, communityRating, supposedRate, rerateMessage, levelID;

    public RatingElementJson RatingInfo { get; private set; }

    public void SetRateInfo(RatingElementJson info)
    {
        artistSong.text = $"{info.Level.Artist} - {info.Level.Song}";
        if (info.CurrentDifficulty != null) managerRating.text = info.CurrentDifficulty.Name;
        else managerRating.text = "?";
        if (info.CommunityDifficulty != null) communityRating.text = info.CommunityDifficulty.Name;
        else communityRating.text = "?";
        levelID.text = "#" + info.LevelID.ToString();

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
