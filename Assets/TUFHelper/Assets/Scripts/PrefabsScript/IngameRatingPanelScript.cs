using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

public class IngameRatingPanelScript : MonoBehaviour
{
    public TextMeshProUGUI songArtist;
    public TMP_InputField yourCommentField, yourRatingField;
    public GameObject parentVotesList, votePrefab;

    public Toggle deleteLevelInput;

    public RatingElementJson RatingInfo { get; private set; }
    public static IngameRatingPanelScript instance { get; private set; }

    public void Awake()
    {
        if (instance == null) instance = this;
    }
    public void SetRatingInfo(RatingElementJson info)
    {
        songArtist.text = $"{info.Level.Song} - {info.Level.Artist}";
        RatingInfo = info;

        int i = 0;
        foreach (var vote in info.Details)
        {
            GameObject obj = Instantiate(votePrefab, parentVotesList.transform);
            RectTransform rect = obj.GetComponent<RectTransform>();

            rect.localScale = Vector3.one;
            rect.anchoredPosition = new Vector2(0, i * -70);

            var script = obj.GetComponent<IngameRatingVotePrefabScript>();
            script.SetVoteInfo(vote);

            //info.Level.ID;

            i++;
        }

        RectTransform contentRect = parentVotesList.GetComponent<RectTransform>();
        float totalHeight = i * 70;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }


    public async void SaveChanges()
    {
        string comment = yourCommentField.text;
        string rating = yourRatingField.text;

        await AccountScript.instance.TokenRequest.TrySendRating(RatingInfo.ID, comment, false, rating);

        scnEditor.instance.TryQuitToMenu();

        if (deleteLevelInput.isOn)
        {

            var level = Main.Setting.DownloadedLevels.FirstOrDefault(e => e.LevelInfo.ID == RatingInfo.Level.ID);

            if (Directory.Exists(level.NameFolder))
            {
                Directory.Delete(level.NameFolder, true);
            }

            Main.Setting.FavoriteLevels.Remove(level.LevelInfo.ID);
            Main.Setting.DownloadedLevels.Remove(level);
        }
    }
}
