using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    public Button saveChangesButton;

    public Toggle deleteLevelInput;

    public RatingElementJson RatingInfo { get; private set; }
    public static IngameRatingPanelScript instance { get; private set; }

    public void Awake()
    {
        instance = this;
    }
    public async Task SetRatingInfo(RatingElementJson info)
    {
        songArtist.text = $"{info.Level.Song} - {info.Level.Artist}";
        LanguageManager.ApplyChineseJapaneseFont(songArtist);
        RatingInfo = info;

        float height = 0;
        foreach (var vote in info.Details)
        {
            GameObject obj = Instantiate(votePrefab, parentVotesList.transform);
            BundleFontFixer.FixFontsIn(obj);
            RectTransform rect = obj.GetComponent<RectTransform>();

            rect.localScale = Vector3.one;
            rect.anchoredPosition = new Vector2(0, -height);

            var script = obj.GetComponent<IngameRatingVotePrefabScript>();
            await script.SetVoteInfo(vote);

            height += script.rectTransform.sizeDelta.y;
        }

        RectTransform contentRect = parentVotesList.GetComponent<RectTransform>();
        float totalHeight = height;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }
    public void TextChanged(string text)
    {
        if (string.IsNullOrEmpty(yourRatingField.text) || string.IsNullOrEmpty(yourCommentField.text))
        {
            saveChangesButton.interactable = false;
        }
        else
        {
            saveChangesButton.interactable = true;
        }
    }


    public async void SaveChanges()
    {
        string comment = yourCommentField.text;
        string rating = yourRatingField.text;

        await AccountScript.instance.TokenRequest.TrySendRating(RatingInfo.ID, comment, false, rating);

        scnEditor.instance.TryQuitToMenu();

        if (deleteLevelInput.isOn)
        {

            //var level = Main.Setting.DownloadedLevels.FirstOrDefault(e => e.LevelInfo.ID == RatingInfo.Level.ID);

            //if (Directory.Exists(level.NameFolder))
            //{
            //    Directory.Delete(level.NameFolder, true);
            //}

            //Main.Setting.FavoriteLevels.Remove(level.LevelInfo.ID);
            //Main.Setting.DownloadedLevels.Remove(level);
        }
    }
}
